using System;
using System.Net.Sockets;

namespace TCPChatGUI.Connection;


public class ClientConnectedEventArgs : EventArgs
{
    public NetworkStream Stream { get; }

    public ClientConnectedEventArgs(NetworkStream stream)
    {
        Stream = stream;
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


