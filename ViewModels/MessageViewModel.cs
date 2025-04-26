using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TCPChatGUI.Models;

namespace TCPChatGUI.ViewModels;

public partial class MessageViewModel : ViewModelBase
{

    [ObservableProperty]
    private string? _Content;

    [ObservableProperty]
    private string? _Username;

    [ObservableProperty]
    private DateTime? _Time;

    public MessageViewModel()
    {
    }

    public MessageViewModel(Message message)
    {
        Content = message.Content;
        Username = message.Username;
        Time = message.Time;
    }

    public Message GetMessage()
    {
        return new Message()
        {
            Content = this.Content,
            Username = this.Username,
            Time = this.Time
        };
    }
}




