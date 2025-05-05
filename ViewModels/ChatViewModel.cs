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

    private readonly UserProfile _localUserProfile;

    private UserProfile _remoteUserProfile = new() { Username = "User" };

    public UserProfile RemoteUserProfile
    {
        get => _remoteUserProfile;
        set
        {
            SetProperty(ref _remoteUserProfile, value);
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string WindowTitle => $"Chatting with {RemoteUserProfile.Username}";

    public ObservableCollection<MessageViewModel> Messages { get; } = [];


    public ChatViewModel(ChatConnection chatConnection, UserProfile localUserProfile)
    {
        _chatConnection = chatConnection;
        _chatConnection.TextReceived += OnTextReceived;
        _chatConnection.DataReceived += OnDataReceived;
        _localUserProfile = localUserProfile;

        _ = _chatConnection.ReadData();
        // _ = Task.Run(ReadConsoleInputAsync);
        _ = SendLocalProfileAsync();
    }


    public void Dispose()
    {
        _chatConnection.TextReceived -= OnTextReceived;
        _chatConnection.DataReceived -= OnDataReceived;
        _chatConnection.Dispose();
    }


    /// <summary>
    /// Returns if a new Message can be send. Messages must have text.
    /// </summary>
    public bool CanSendMessage()
    {
        return !string.IsNullOrWhiteSpace(NewMessageContent);
    }


    /// <summary>
    /// Handle received data by the <see cref="ChatConnection.DataReceived"/> event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDataReceived(object? sender, Connection.DataReceivedEventArgs e)
    {
        try
        {
            // Reconstrói o objeto packet recebido pela conexão
            string dataJson = Encoding.UTF8.GetString(e.Data);
            Packet? packet = JsonSerializer.Deserialize<Packet>(dataJson);

            if (packet == null || dataJson == null || packet.Payload == null) return;

            // Analisa o tipo de packet recebido
            switch (packet.Type)
            {   
                // Caso mensagem:
                case Packet.PacketType.Message:
                
                    // Reconstrói o objeto Message contido no payload.
                    Message? message = JsonSerializer.Deserialize<Message>(packet.Payload);
                    if (message == null) break;

                    Messages.Add(new MessageViewModel(message));

                    // Dispara o evento de nova mensagem recebida.
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

                    Console.WriteLine($"Message received: {message.User?.Username}: {message.Content} - {message.Time}");
                    break;

                // Caso perfil de usuário:
                case Packet.PacketType.UserProfile:

                    // Reconstrói o objeto UserProfile contido no payload.
                    UserProfile? userProfile = JsonSerializer.Deserialize<UserProfile>(packet.Payload);
                    if (userProfile == null) break;

                    RemoteUserProfile = userProfile;
                    break;

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing received data: {ex}");
        }
    }

    /// <summary>
    /// Send the message through the <see cref="ChatConnection.WriteData(byte[])"/> method.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns> 
    private async Task SendMessageAsync(Message message)
    {
        try
        {
            // Converte a mensagem em uma string json.
            string messageJson = JsonSerializer.Serialize(message);

            // Cria um pacote do tipo mensagem com a mensagem em formato json.
            Packet packet = new ()
            {
                Type = Packet.PacketType.Message,
                Payload = messageJson
            };

            // Converte o pacote em uma string json.
            string packetJson = JsonSerializer.Serialize(packet);
            // Converte a string em uma série de bytes.
            byte[] dataToSend = Encoding.UTF8.GetBytes(packetJson);

            // Aguarda a Task assíncrona da classe ChatConnection enviar os dados pela stream.
            await _chatConnection.WriteData(dataToSend);
        }
        catch (ObjectDisposedException ex)
        {
            Error?.Invoke(this, new ErrorEventArgs("This connection is no longer available.", ex));
            Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending message asynchronously: {ex}");
        }
    }


    /// <summary>
    /// Send message and add it to the list. Only available if <see cref="CanSendMessage"/> is true.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private void SendMessage()
    {
        // Constrói o objeto mensagem.
        Message message = new()
        {
            User = _localUserProfile,
            Content = NewMessageContent,
            Time = DateTime.Now
        };

        // Aciona a Task para enviar a mensagem de forma assíncrona.
        _ = SendMessageAsync(message);

        // Adiciona a mensagem à coleção de mensagens
        MessageViewModel viewModel = new(message);
        Messages.Add(viewModel);

        // Reseta o campo de conteúdo da mensagem.
        NewMessageContent = null;
    }


    private async Task SendLocalProfileAsync()
    {   
        // Converte o perfil local em uma string json.
        string profileJson = JsonSerializer.Serialize(_localUserProfile);

        // Cria um pacote do tipo perfil de usuário com o perfil em formato json.
        Packet packet = new ()
        {
            Type = Packet.PacketType.UserProfile,
            Payload = profileJson
        };

        // Converte o pacote em uma string json.
        string packetJson = JsonSerializer.Serialize(packet);
        // Converte a string em uma série de bytes.
        byte[] dataToSend = Encoding.UTF8.GetBytes(packetJson);
        // Aguarda a Task assíncrona da classe ChatConnection enviar os dados pela stream.
        await _chatConnection.WriteData(dataToSend);
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
            string? input = Console.ReadLine();
            // Ignora inputs vazios.
            if (string.IsNullOrWhiteSpace(input)) continue;

            // Bloqueia a thread até que o texto seja enviado ao stream.
            await _chatConnection.WriteText(input);
        }
    }


    // public event EventHandler<TextReceivedEventArgs>? TextReceived;

    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    public event EventHandler<ErrorEventArgs>? Error;



}
