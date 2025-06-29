using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using TCPChatGUI.Models;

namespace TCPChatGUI.Connection;

public class ChatClient : TcpClient, IDisposable
{
    private bool _disposed = false;

    public bool Disposed {get => _disposed;}
   
    /// <summary>
    /// Tenta se conectar a um socket TCP específico (servidor).
    /// Emite um evento <see cref="OnClientConnected"/> em caso de sucesso.
    /// </summary>
    /// <param name="iPEndPoint">EndPoint do servidor.</param>
    public void ConnectToServer(IPEndPoint iPEndPoint)
    {
        // Lança um erro caso iPEndPoint seja nulo.
        ArgumentNullException.ThrowIfNull(iPEndPoint);

        Debug.WriteLine($"Connecting to {iPEndPoint.Address}:{iPEndPoint.Port}...");
        
        try
        { 
            Client.Connect(iPEndPoint);
            NetworkStream Stream = GetStream();
            ConfigureKeepAlive(Client);

            Debug.WriteLine("Connected to server.");

            ClientConnected?.Invoke(this, new ClientConnectedEventArgs(Stream, iPEndPoint));
        }
        catch (ObjectDisposedException ex)
        {
            Debug.WriteLine($"Tried accessing a disposed objetc: {ex}");
            throw;
        }

        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostUnreachable)
        {
            Debug.WriteLine("Tried connecting to an unreachable server.");
            throw;
        }
        catch (SocketException ex)
        {
            Debug.WriteLine($"Socket error: \n{ex}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error connecting to server: \n{ex}");
            throw;
        }
        finally 
        {
            Dispose();
        }

    }


    
    /// <summary>
    /// Tenta se conectar a um socket TCP específico (servidor).
    /// Emite um evento <see cref="OnClientConnected"/> em caso de sucesso.
    /// </summary>
    /// <param name="iPAddress"></param>
    /// <param name="port"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ConnectToServer(IPAddress iPAddress, int port)
    {
        ArgumentNullException.ThrowIfNull(iPAddress);

        if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port));


        ConnectToServer(new IPEndPoint(iPAddress, port));
    }

    new public void Dispose()
    {
        if (Disposed) return;

            
        _disposed = true;
        GC.SuppressFinalize(this);
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


}


