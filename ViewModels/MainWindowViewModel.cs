using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TCPChatGUI.Connection;
using TCPChatGUI.Views;

namespace TCPChatGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{


    private readonly ObservableCollection<ChatClient> _clients = [];


    [ObservableProperty]
    private ChatServer _chatServer;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IpLabel))]
    private IPAddress? _ip;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortLabel))]
    private int? _port;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _inputIPString = "127.0.1.1";


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _inputPortString = "11000";

    [ObservableProperty]
    private string _inputUsername = "User";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BecomeAvailableButtonLabel))]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    private bool _serverAvailable;

    public UserProfileViewModel LocalUser;

    public string IpLabel => Ip?.ToString() ?? "N/A";
    public string PortLabel => Port?.ToString() ?? "N/A";


    public string StatusLabel => ServerAvailable ? "Available" : "Unavailable";

    public string BecomeAvailableButtonLabel => ServerAvailable ? "Become unavailable" : "Become available";


    public ObservableCollection<ChatConnection> Connections { get; } = [];



    public MainWindowViewModel()
    {
        LocalUser = new();
        ChatServer = new ChatServer();
        ChatServer.ClientConnected += OnClientConnected;
        ChatServer.ServerStatusChanged += OnServerStatusChanged;
    }


    /// <summary>
    /// Verifica se o IP e port fornecidos pelo usuário são válidos.
    /// </summary>
    /// <returns></returns>
    public bool CanConnect()
    {
        if (!IPAddress.TryParse(InputIPString, out var ipAddress))
            return false;


        if (!int.TryParse(InputPortString, out int port) || port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            return false;

        return true;
    }


    /// <summary>
    /// Tenta conectar o cliente ao endereço fornecido pelas textboxes.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConnect))]
    public void Connect()
    {
        var ipAddress = IPAddress.Parse(InputIPString);
        var port = int.Parse(InputPortString);
        var targetEndPoint = new IPEndPoint(ipAddress, port);

        // Verifica as conexões existentes.
        foreach (var connection in Connections.ToList())
        {
            // Remove conexões descartadas
            if (connection.Disposed)
            {
                DisposeConnection(connection);
                continue;
            }

            var connEndPoint = connection.ConnectionEndPoint;
            // Não conecta em servidores que já está conectado.
            if (targetEndPoint.Equals(connEndPoint))
            {
                Error?.Invoke(this, new ErrorEventArgs("Already connected to that user.", null));
                return;
            }
        }
        try
        {
            GetAvailableClient().ConnectToServer(ipAddress, port);
        }
        catch (SocketException ex)
        {
            Error?.Invoke(this, new ErrorEventArgs("Connection error: " + ex.Message, ex));
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, new ErrorEventArgs("Unexpected error: " + ex.Message, ex));
        }


    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ToggleServerAvailability()
    {
        if (!ChatServer.ListenerCreated)
        {
            await CreateServer();
            return;
        }

        // Interrompe o servidor caso esteja disponível e vice-versa.
        if (ChatServer.Available)
        {
            try
            {
                ChatServer.StopServer();
                ServerAvailable = false;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new ErrorEventArgs("Unexpected error while toggling availability: " + ex.Message, ex));
            }
        }
        else
        {
            try
            {
                ChatServer.StartServer();
                ServerAvailable = true;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new ErrorEventArgs("Unexpected error while toggling availability: " + ex.Message, ex));
            }
        }
    }

    /// <summary>
    /// Cria o servidor, exibe o IP e port na interface e inicia a Task <see cref="ChatServer.AcceptClient"/>
    /// </summary>
    /// <returns></returns>
    public async Task CreateServer()
    {
        await ChatServer.CreateServer();
        Ip = ChatServer.ServerIp;
        Port = ChatServer.ServerPort;
    }


    public ChatClient CreateClient()
    {
        var newClient = new ChatClient();
        newClient.ClientConnected += OnClientConnected;
        return newClient;
    }


    public void DisposeClient(ChatClient client)
    {
        client.ClientConnected -= OnClientConnected;
        client.Dispose();
        _clients.Remove(client);
    }

    public void DisposeServer(ChatServer server)
    {
        server.Dispose();
        server.ClientConnected -= OnClientConnected;
    }

    private void DisposeConnection(ChatConnection connection)
    {
        connection.Dispose();
        Connections.Remove(connection);
    }


    /// <summary>
    /// Descarta clientes mortos e retorna um <see cref="ChatClient"/> disponível para conexão da lista de clientes.
    /// Se nenhum cliente estiver disponível, cria um novo e o retorna.
    /// </summary>
    /// <returns></returns>
    private ChatClient GetAvailableClient()
    {
        // Instancia um novo cliente caso a lista não contenha nenhum.
        if (_clients.Count == 0)
        {
            _clients.Add(CreateClient());
            return _clients[0];
        }

        // Itera sobre a lista e seleciona um cliente que NÃO esteja conectado e que NÃO esteja morto.
        foreach (var client in _clients.ToList())
        {
            if (!client.Connected)
            {
                if (client.Disposed)
                {
                    // Descarta clientes mortos.
                    DisposeClient(client);
                }
                else
                {
                    return client;
                }
            }
        }

        // Instancia um novo cliente pois nenhum outro estava disponível.
        var newClient = CreateClient();
        _clients.Add(newClient);
        return newClient;
    }


    /// <summary>
    /// Instancia uma nova <see cref="ChatConnection"/> e dispara o evento <see cref="NewChatConnection"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        var connection = new ChatConnection(e.Stream, e.EndPoint);
        Connections.Add(connection);
        NewChatConnection?.Invoke(this, new NewChatConnectionEventArgs(connection, LocalUser.GetUserProfile()));

    }

    private void OnServerStatusChanged(object? sender, ServerStatusChangedEventArgs e)
    {
        ServerAvailable = e.Available;
    }

    // Atualiza o nome do usuário no perfil local.
    partial void OnInputUsernameChanged(string value)
    {
        LocalUser.UpdateUsername(value);
    }


    public event EventHandler<NewChatConnectionEventArgs>? NewChatConnection;

    public event EventHandler<ErrorEventArgs>? Error;
}
