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
    private UserProfile? _user;

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
        User = message.User;
    }

    public Message GetMessage()
    {
        return new Message()
        {
            Content = this.Content,
            User = this.User,
            Username = this.Username,
            Time = this.Time
        };
    }
}




