using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TCPChatGUI.Connection;
using TCPChatGUI.Views;

namespace TCPChatGUI.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{

    public ObservableCollection<ChatConnection> Connections { get; } = [];

    // TODO: Criar lista de clientes (cada cliente só pode se conectar a um servidor por vez)


    [ObservableProperty]
    private ChatServer _ChatServer;


    [ObservableProperty]
    private ChatClient _ChatClient;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IpLabel))]
    private IPAddress? _Ip;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PortLabel))]
    private int? _Port;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _inputIPString = "127.0.1.1";


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _inputPortString = "11000";


    public string IpLabel => $"IP: {Ip}";
    public string PortLabel => $"Port: {Port}";



    public MainWindowViewModel()
    {
        ChatServer = new ChatServer();
        ChatClient = new ChatClient();

        ChatServer.ClientConnected += OnClientConnected;
        ChatClient.ClientConnected += OnClientConnected;
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
        ChatClient.ConnectToServer(ipAddress, port);
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


    /// <summary>
    /// Instancia um novo chat contendo a conexão fornecida.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        var connection = new ChatConnection(e.Stream);
        // TODO: lógica de remoção de conexões; instanciar novo cliente;
        Connections.Add(connection);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Chat chatWindow = new(connection);
            chatWindow.Show();

        });
    }



    public void Dispose()
    {
        ChatServer.ClientConnected -= OnClientConnected;
        ChatClient.ClientConnected -= OnClientConnected;
    }



}
