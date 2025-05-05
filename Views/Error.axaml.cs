using Classic.Avalonia.Theme;
using TCPChatGUI.ViewModels;
using TCPChatGUI.Connection;
using TCPChatGUI.Models;
using Avalonia.Threading;
using Avalonia.Controls;
using System.Diagnostics;

namespace TCPChatGUI.Views;

public partial class Error : ClassicWindow
{   
    private readonly ErrorViewModel _viewModel;
    public Error()
    {
        InitializeComponent();
        _viewModel = new();
        DataContext = _viewModel;
    }

    public Error(string errorMessage)
    {
        InitializeComponent();
        _viewModel = new(errorMessage);
        DataContext = _viewModel;
    }
}