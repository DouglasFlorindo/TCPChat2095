using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Classic.Avalonia.Theme;
using TCPChatGUI.ViewModels;
using TCPChatGUI.Connection;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace TCPChatGUI.Views;

public partial class Chat : ClassicWindow
{
    private readonly ChatViewModel ViewModel;

    public Chat(ChatConnection chatConnection) {;
        
        InitializeComponent();
        ViewModel = new(chatConnection);
        DataContext = ViewModel;
        
    }

    public void OnTextReceived(object? sender, TextReceivedEventArgs e)
    {}

}