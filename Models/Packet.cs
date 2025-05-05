using System;

namespace TCPChatGUI.Models;

public class Packet
{
    public enum PacketType
    {
        Message,
        UserProfile
    }

    public PacketType? Type { get; set; }


    public string? Payload { get; set; } = string.Empty;

}