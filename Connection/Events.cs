using System;
using System.Net;
using System.Net.Sockets;

namespace TCPChatGUI.Connection;


public class ClientConnectedEventArgs : EventArgs
{
    public NetworkStream Stream { get; }
    public IPEndPoint EndPoint;

    public ClientConnectedEventArgs(NetworkStream stream, IPEndPoint endPoint)
    {
        Stream = stream;
        EndPoint = endPoint;
    }
}


public class TextReceivedEventArgs : EventArgs
{

    public string Text { get; }

    public TextReceivedEventArgs(string text)
    {
        Text = text;
    }

}


