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

    public Chat(ChatConnection chatConnection) {;
        
        InitializeComponent();
        DataContext = new ChatViewModel(chatConnection);
            
        
    }

    public Chat() {
        InitializeComponent();
    }

}