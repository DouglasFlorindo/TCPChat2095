using System;
using System.Net;
using System.Net.Sockets;
using TCPChatGUI.Models;

namespace TCPChatGUI.Connection;


public class ServerStatusChangedEventArgs(bool available) : EventArgs
{
    public bool Available { get; } = available;
}

public class ClientConnectedEventArgs(NetworkStream stream, IPEndPoint endPoint) : EventArgs
{
    public NetworkStream Stream { get; } = stream;
    public IPEndPoint EndPoint { get; } = endPoint;
}


public class NewChatConnectionEventArgs(ChatConnection connection, UserProfile userProfile) : EventArgs
{
    public ChatConnection Connection { get; } = connection;
    public UserProfile UserProfile { get; } = userProfile;
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

public class ErrorEventArgs(string? errorMessage, Exception? errorException) : EventArgs
{
    public string? ErrorMessage { get; } = errorMessage;
    public Exception? ErrorException { get; } = errorException;
}