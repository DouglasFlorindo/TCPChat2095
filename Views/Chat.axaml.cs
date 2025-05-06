using Classic.Avalonia.Theme;
using TCPChatGUI.ViewModels;
using TCPChatGUI.Connection;
using TCPChatGUI.Models;
using Avalonia.Threading;
using Avalonia.Controls;
using System.Diagnostics;
using System;


namespace TCPChatGUI.Views;

public partial class Chat : ClassicWindow
{
    private readonly ChatViewModel _viewModel;

    private readonly UserProfile _localUserProfile;

    public Chat(ChatConnection chatConnection, UserProfile userProfile)
    {
        InitializeComponent();
        _localUserProfile = userProfile;
        _viewModel = new(chatConnection, _localUserProfile);
        DataContext = _viewModel;

        Loaded += (_, _) =>
        {
            _viewModel.Error += OnError;
            _viewModel.Messages.CollectionChanged += (_, args) =>
            {
                Dispatcher.UIThread.InvokeAsync(() => MessagesScrollViewer?.ScrollToEnd());
            };
        };
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


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Debug.WriteLine("Closing chat window...");
        _viewModel.Error -= OnError;
        _viewModel.Dispose();
        base.OnClosing(e);
    }
}