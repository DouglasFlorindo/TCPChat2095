using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        viewModel.ClientConnected += (sender, connection) =>
        {
            var chatWindow = new Chat(connection);
            chatWindow.Show();
        };
        DataContext = viewModel;
    }
}
