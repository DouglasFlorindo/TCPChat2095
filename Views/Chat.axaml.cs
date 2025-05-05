using Classic.Avalonia.Theme;
using TCPChatGUI.ViewModels;
using TCPChatGUI.Connection;
using TCPChatGUI.Models;
using Avalonia.Threading;
using Avalonia.Controls;
using System.Diagnostics;


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

        this.Loaded += (_, _) =>
        {
            if (DataContext is ChatViewModel vm)
            {
                vm.Messages.CollectionChanged += (_, args) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        MessagesScrollViewer?.ScrollToEnd();
                    });
                };
            }
        };
    }


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Debug.WriteLine("Closing chat window...");
        _viewModel.Dispose();
    }
}