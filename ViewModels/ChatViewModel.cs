using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
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

    public ChatViewModel(ChatConnection chatConnection)
    {
        _chatConnection = chatConnection;
        _chatConnection.TextReceived += OnTextReceived;

        // _ = _chatConnection.ReadText();
        _ = Task.Run(() => ReadConsoleInputAsync());
    }

    /// <summary>
    /// Lida com o texto emitido pelo evento <see cref="TextReceived"/>.
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


    public EventHandler<TextReceivedEventArgs>? TextReceived;



    // =================== TO-DO ===========================



    // private void OnDataReceived()
    // {

    // }


    // private async Task SendMessage(Message message)
    // {

    // }





















    // ================ ignora isso por enquanto =================




    public ObservableCollection<MessageViewModel> Messages { get; } = [];

    /// <summary>
    /// Add new Message to the list. Only available if CanAddMessage is true.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddMessage))]
    public void AddMessage()
    {
        Messages.Add(new MessageViewModel() { Username = "[User]", Content = NewMessageContent, Time = DateTime.Now });
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


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddMessageCommand))] // This attribute will invalidate the command each time this property changes
    private string? _NewMessageContent;



    /// <summary>
    /// Returns if a new Message can be added. Messages must have text
    /// </summary>
    public bool CanAddMessage()
    {
        return !string.IsNullOrWhiteSpace(NewMessageContent);
    }



}
