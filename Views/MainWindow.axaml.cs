using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using TCPChatGUI.Connection;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel ViewModel;
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new();
        ViewModel.NewChatConnection += OnNewChatConnection;
        DataContext = ViewModel;
    }

    public void OnNewChatConnection(object? sender, NewChatConnectionEventArgs e)
    {
        var chatWindow = new Chat(e.Connection);
        chatWindow.Show();
    }
}
