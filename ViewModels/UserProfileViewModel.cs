using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using TCPChatGUI.Models;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.ViewModels;

public partial class UserProfileViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _username = "User";

    public UserProfileViewModel()
    {
    }

    public UserProfileViewModel(UserProfile userProfile)
    {
        Username = userProfile.Username;
    }


    public void UpdateUsername(string username)
    {
        var currentUsername = Username;

        try
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                Username = username;
            }
            else
            {
                Username = "User";
            }
        }
        catch (System.Exception)
        {
            Username = currentUsername;
        }
    }


    public UserProfile GetUserProfile()
    {
        return new UserProfile()
        {
            Username = Username
        };
    }
}