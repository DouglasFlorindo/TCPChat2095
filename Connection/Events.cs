using System;
using System.Net;
using System.Net.Sockets;
using TCPChatGUI.Models;

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


public class DataReceivedEventArgs(byte[] data) : EventArgs
{
    public byte[] Data { get; } = data;
}

public class MessageReceivedEventArgs(Message message) : EventArgs
{
    public Message Message { get; } = message;
}