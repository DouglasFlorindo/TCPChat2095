using System;
using System.Net;
using System.Net.Sockets;

namespace TCPChatGUI.Connection;


public class ClientConnectedEventArgs(NetworkStream stream, IPEndPoint endPoint) : EventArgs
{
    public NetworkStream Stream { get; } = stream;
    public IPEndPoint EndPoint = endPoint;
}


public class NewChatConnectionEventArgs(ChatConnection connection) : EventArgs
{
    public ChatConnection Connection { get; } = connection;
}


public class TextReceivedEventArgs(string text) : EventArgs
{

    public string Text { get; } = text;
}


