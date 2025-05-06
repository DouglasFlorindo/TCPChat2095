using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TCPChatGUI.ViewModels;
public partial class ErrorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _errorMessage = "Lorem ipsum dolor sit amet";

    public ErrorViewModel()
    {
    }

    public ErrorViewModel(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
