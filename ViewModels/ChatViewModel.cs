using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TCPChatGUI.Connection;
using TCPChatGUI.Models;
using TCPChatGUI.Views;

namespace TCPChatGUI.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    /// <summary>
    /// <see cref="ChatConnection"/> ao qual o chat está associado.
    /// </summary>
    private readonly ChatConnection _chatConnection;

    /// <summary>
    /// Conteúdo da mensagem a ser enviada. Sempre que o conteúdo desta propriedade é alterado, o comando <see cref="SendMessage"/> é invalidado (e então será validado ou não pelo método <see cref="CanSendMessage"/>) 
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string? _newMessageContent;

    public ObservableCollection<MessageViewModel> Messages { get; } = [];

    


    public ChatViewModel(ChatConnection chatConnection)
    {
        _chatConnection = chatConnection;
        _chatConnection.TextReceived += OnTextReceived;
        _chatConnection.DataReceived += OnDataReceived;

        _ = _chatConnection.ReadData();
        _ = Task.Run(() => ReadConsoleInputAsync());
    }

    /// <summary>
    /// Lida com o texto emitido pelo evento <see cref="TextReceived"/> (emitido pela classse <see cref="ChatConnection"/>).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTextReceived(object? sender, TextReceivedEventArgs e)
    {
        Console.WriteLine($"- {e.Text}");
    }

    /// <summary>
    /// Inicia a leitura do console e envia o input para o stream por meio do método <see cref="ChatConnection.WriteText(string)"/>. 
    /// </summary>
    /// <returns></returns>
    private async Task ReadConsoleInputAsync()
    {
        while (true)
        {
            var input = Console.ReadLine();
            // Ignora inputs vazios.
            if (string.IsNullOrWhiteSpace(input)) continue;

            // Bloqueia a thread até que o texto seja enviado ao stream.
            await _chatConnection.WriteText(input);
        }
    }

    /// <summary>
    /// Lida com os dados emitidos pelo evento <see cref="DataReceived"/> (emitido pela classse <see cref="ChatConnection"/>).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDataReceived(object? sender, Connection.DataReceivedEventArgs e)
    {
        try
        {
            string jsonMessage = Encoding.UTF8.GetString(e.Data);
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);

            if (message != null)
            {
                Messages.Add(new MessageViewModel(message));

                // Dispara o evento de nova mensagem recebida.
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

                Console.WriteLine($"Mensagem recebida: {message.Username}: {message.Content} - {message.Time}");
            }
            else
            {
                Console.WriteLine("Mensagem recebida não é válida.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar dados recebidos: {ex.Message}");
        }
    }


    private async Task SendMessage(Message message)
    {
        string jsonMessage = JsonSerializer.Serialize(message);

        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonMessage);

        await _chatConnection.WriteData(dataToSend);
    }


    /// <summary>
    /// Send message and add it to the list. Only available if <see cref="CanSendMessage"/> is true.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    public void SendMessage()
    {
        var message = new Message
        {
            Username = "[User]",
            Content = NewMessageContent,
            Time = DateTime.Now
        };

        _ = SendMessage(message); 

        var viewModel = new MessageViewModel(message);
        Messages.Add(viewModel);

        NewMessageContent = null;
    }

    /// <summary>
    /// Remove a specific Message from the list.
    /// </summary>
    [RelayCommand]
    public void RemoveMessage(MessageViewModel message)
    {
        Messages.Remove(message);
    }


    



    /// <summary>
    /// Returns if a new Message can be send. Messages must have text.
    /// </summary>
    public bool CanSendMessage()
    {
        return !string.IsNullOrWhiteSpace(NewMessageContent);
    }


    public EventHandler<TextReceivedEventArgs>? TextReceived;

    public EventHandler<MessageReceivedEventArgs>? MessageReceived;



}
