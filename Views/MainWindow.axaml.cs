using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Diagnostics;
using TCPChatGUI.Connection;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        // Garante que os eventos sejam linkados depois que o DataConext carregue.
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.NewChatConnection += OnNewChatConnection;
            viewModel.Error += OnError;
        }
        this.Loaded -= OnWindowLoaded; 
    }

    private async void OnNewChatConnection(object? sender, NewChatConnectionEventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var chatWindow = new Chat(e.Connection, e.UserProfile);
            chatWindow.Show();
        });
    }

    public void OnError(object? sender, ErrorEventArgs e)
    {
        if (string.IsNullOrEmpty(e.ErrorMessage)) return;

        // Exibe janela de erro somente se evento contém mensagem de erro.
        var errorWindow = new Error(e.ErrorMessage);
        errorWindow.Show();
    }
}
