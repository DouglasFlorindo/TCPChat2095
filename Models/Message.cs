
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TCPChatGUI.Models;

/// <summary>
/// Model da mensagem.
/// </summary>
public partial class Message : ObservableObject
{
    public string? Content;

    public string? Username;

    public DateTime? Time;
 
}