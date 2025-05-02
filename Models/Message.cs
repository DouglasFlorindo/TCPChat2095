using System;

namespace TCPChatGUI.Models;

/// <summary>
/// Model da mensagem.
/// </summary>
public class Message
{
    public string? Content { get; set; }

    public string? Username { get; set; }

    public DateTime? Time { get; set; }

}