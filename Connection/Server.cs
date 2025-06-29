using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TCPChatGUI.Connection;

public partial class ChatServer : ObservableObject, IDisposable
{
    private IPEndPoint _endPoint;
    private TcpListener? _listener;

    private readonly List<TcpClient> _clients = [];

    private bool _disposed;

    private bool _available;

    public bool ListenerCreated { get; private set; } = false;
    public bool Available
    {
        get => _available;
        private set
        {
            if (_available != value)
            {
                _available = value;
                ServerStatusChanged?.Invoke(this, new ServerStatusChangedEventArgs(value));
            }
        }
    }

    public IPAddress? ServerIp { get => _endPoint.Address; }

    public int? ServerPort { get => _endPoint.Port; }

    public ChatServer()
    {
        _endPoint = new(IPAddress.Loopback, 0);
    }

    /// <summary>
    /// Cria um <see cref="TcpListener"/> no IP local da máquina.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Nenhum port está disponível.</exception>
    public async Task CreateServer()
    {
        Debug.WriteLine("Creating server...");

        var hostName = Dns.GetHostName();
        IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
        var ipAddresses = localhost.AddressList;

        IPAddress localIpAddress = NetworkUtils.GetLocalIPv4();

        int port = 11000;
        bool isBound = false;

        // Loop para encontrar outro port livre caso o port especificado esteja em uso.
        while (!isBound && port <= 65535)
        {
            _endPoint = new IPEndPoint(localIpAddress, port);
            _listener = new TcpListener(_endPoint);

            try
            {
                StartServer();
                isBound = true;
                ListenerCreated = true;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                port++;
            }
        }

        // Lança um erro caso nenhum port esteja livre.
        if (!isBound)
            throw new InvalidOperationException("No available ports found.");

        Debug.WriteLine($"Server initiated in {localIpAddress}:{port}.");
    }

    /// <summary>
    /// Inicia o <see cref="TcpListener"/>.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public void StartServer()
    {
        Debug.WriteLine($"Starting server...");
        if (_listener == null)
            throw new InvalidOperationException("Server was not initialized yet.");
        try
        {
            _listener.Start();
            Available = true;

            _ = AcceptClient();
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            throw;
        }
        catch (System.Exception)
        {
            // TODO
            throw;
        }
    }

    /// <summary>
    /// Interrompe o <see cref="TcpListener"/>.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public void StopServer()
    {
        Debug.WriteLine($"Stopping server...");
        if (_listener == null)
            throw new InvalidOperationException("Server was not initialized yet.");
        try
        {
            _listener.Stop();
            Available = false;

        }
        catch (System.Exception)
        {
            // TODO
            throw;
        }
    }

    /// <summary>
    /// Entra em um loop para aguardar a conexão de clientes.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task AcceptClient()
    {
        while (Available == true)
        {
            if (_listener == null)
                throw new InvalidOperationException("Server was not created yet.");


            try
            {
                // Bloqueia a thread até a conexão de um novo cliente.
                var client = await _listener.AcceptTcpClientAsync();

                _ = Task.Run(() =>
                {
                    HandleClient(client);
                });

                Debug.WriteLine("Server accepted a new client.");

            }
            catch (SocketException ex)
            {
                Debug.WriteLine($"Socket error: {ex}");
                break;
                throw;
            }
        }
    }


    public void Dispose()
    {
        if (!_disposed)
        {
            StopServer();
            foreach (var client in _clients)
                client.Dispose();
            _disposed = true;

        }
    }


    /// <summary>
    /// Emite um evento <see cref="OnClientConnected"/>.
    /// </summary>
    /// <param name="client"></param>
    private void HandleClient(TcpClient client)
    {
        Debug.WriteLine("Handling client...");

        ConfigureKeepAlive(client.Client);
        _clients.Add(client);
        var stream = client.GetStream();
        ClientConnected?.Invoke(this, new ClientConnectedEventArgs(stream, _endPoint));
    }


    private static void ConfigureKeepAlive(Socket socket)
    {

        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ConfigureWindowsKeepAlive(socket);
        }
        else
        {
            ConfigureUnixKeepAlive(socket);
        }
    }


    private static void ConfigureWindowsKeepAlive(Socket socket)
    {
        uint time = 30000;
        uint interval = 5000;

        byte[] inValue = new byte[12];
        BitConverter.GetBytes(1u).CopyTo(inValue, 0);
        BitConverter.GetBytes(time).CopyTo(inValue, 4);
        BitConverter.GetBytes(interval).CopyTo(inValue, 8);

#pragma warning disable CA1416 // Validar a compatibilidade da plataforma
        socket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
#pragma warning restore CA1416 // Validar a compatibilidade da plataforma
    }


    private static void ConfigureUnixKeepAlive(Socket socket)
    {
        int tcpKeepIdle = 30;
        int tcpKeepIntvl = 5;
        int tcpKeepCnt = 5;

        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, tcpKeepIntvl);
        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, tcpKeepIdle);
        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, tcpKeepCnt);
    }

    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    public event EventHandler<ServerStatusChangedEventArgs>? ServerStatusChanged;
}

