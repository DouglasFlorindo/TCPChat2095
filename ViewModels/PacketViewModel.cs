using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using TCPChatGUI.Models;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.ViewModels;

public partial class PacketViewModel : ViewModelBase
{
    [ObservableProperty]
    private Packet.PacketType? _type;

    [ObservableProperty]
    private string? _payload;

    public PacketViewModel()
    {
    }

    public PacketViewModel(Packet packet)
    {
        Type = packet.Type;
        Payload = packet.Payload;
    }


    public Packet GetPacket()
    {
        return new Packet()
        {
            Type = Type,
            Payload = Payload
        };
    }
}