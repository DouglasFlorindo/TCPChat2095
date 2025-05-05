using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TCPChatGUI.ViewModels;
public partial class ErrorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ErrorViewModel()
    {
    }

    public ErrorViewModel(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
