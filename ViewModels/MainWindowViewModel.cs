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

    public ObservableCollection<ChatConnection> Connections { get; } = [];

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


    public string IpLabel => $"{Ip}";
    public string PortLabel => $"{Port}";



    public MainWindowViewModel()
    {
        ChatServer = new ChatServer();
        ChatServer.ClientConnected += OnClientConnected;
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
        foreach (var connection in Connections)
        {
            var connEndPoint = connection.ConnectionEndPoint;
            // Não conecta em servidores que já está conectado.
            if (targetEndPoint.Equals(connEndPoint))
            {
                throw new InvalidOperationException("Already connected to that server.");
            }
        }
        GetAvailableClient().ConnectToServer(ipAddress, port);
    }


    /// <summary>
    /// Cria o servidor, exibe o IP e port na interface e inicia a Task <see cref="ChatServer.AcceptClient"/>
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task InitializeServer()
    {
        await ChatServer.CreateServer();
        Ip = ChatServer.ServerIp;
        Port = ChatServer.ServerPort;
        await ChatServer.AcceptClient();
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
        client.Dispose(true);
        _clients.Remove(client);
    }

    public void DisposeServer(ChatServer server)
    {   
        server.Dispose();
        server.ClientConnected -= OnClientConnected;
    }


    /// <summary>
    /// Instancia um novo chat contendo a conexão fornecida.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        var connection = new ChatConnection(e.Stream, e.EndPoint);
        // TODO: lógica de remoção de conexões;
        Connections.Add(connection);
        NewChatConnection?.Invoke(this, new NewChatConnectionEventArgs(connection));


        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Chat chatWindow = new(connection);
            chatWindow.Show();

        });
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
        foreach (var client in _clients)
        {
            if (!client.Connected)
            {
                if (client.IsDead)
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


    public event EventHandler<NewChatConnectionEventArgs>? NewChatConnection;


}
