using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using TCPChatGUI.Connection;
using TCPChatGUI.ViewModels;

namespace TCPChatGUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            // Garante que os eventos sejam linkados depois que o DataConext carregue.
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.NewChatConnection += OnNewChatConnection;
                viewModel.Error += OnError;
            }
        };
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
        try
        {
            // Exibe janela de erro somente se o evento contém mensagem de erro.
            if (string.IsNullOrEmpty(e.ErrorMessage)) return;
            var errorWindow = new Error(e.ErrorMessage);
            errorWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error displaying error window: {ex}");
        }
    }
}
