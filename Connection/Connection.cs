using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TCPChatGUI.Connection;

/// <summary>
/// Contém o <see cref="NetworkStream"/> de uma conexão e implementa funções de comunicação entre cliente e servidor. 
/// </summary>
public partial class ChatConnection : ObservableObject
{
    [ObservableProperty]
    private NetworkStream _stream;

    private readonly StreamReader _textReader;

    private readonly StreamWriter _textWriter;

    /// <summary>
    /// Token responsável por encerrar a leitura do stream.
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _disposed;

    
    public ChatConnection(NetworkStream stream)
    {
        Debug.WriteLine("Connection established.");
        Stream = stream;


        _textReader = new(Stream, Encoding.UTF8, leaveOpen: true);
        _textWriter = new(Stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

        _ = Task.Run(() => ReadText());
    }


    /// <summary>
    /// Inicia a leitura exclusiva de texto por meio do <see cref="StreamReader"/>. Emite o evento <see cref="OnTextReceived"/> quando lê um texto no stream. 
    /// </summary>
    /// <returns></returns>
    public async Task ReadText()
    {
        var buffer = new char[1024];

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Bloqueia a thread até que leia algo no stream.
                int bytesRead = await _textReader.ReadAsync(buffer, 0, buffer.Length);
                // Encerra a conexão quando lê exatamente 0 bytes (sinal de conexão encerrada).
                if (bytesRead == 0)
                {
                    Debug.WriteLine("Connection closed by remote host");
                    break;
                }

                string text = new(buffer, 0, bytesRead);
                // Emite o evento de texto recebido
                OnTextReceived(text.TrimEnd('\0'));
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Read operation canceled");
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex}");
        }
        finally
        {
            CloseConnection();
        }
    }


    /// <summary>
    /// Encerra a leitura do stream.
    /// </summary>
    public void StopReading()
    {
        _cancellationTokenSource.Cancel();
    }


    /// <summary>
    /// Escreve exclusivamente um texto no stream por meio do <see cref="StreamWriter"/> .
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task WriteText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            // Bloqueia a thread até que o texto seja entregue ao stream.
            await _textWriter.WriteLineAsync(text);
        }
        catch (ObjectDisposedException)
        {
            Debug.WriteLine("Attempted to write in a closed connection");
            CloseConnection();
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"Network write error: {ex.Message}");
            CloseConnection();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Write error: {ex}");
        }
    }

    /// <summary>
    /// Encerra a conexão
    /// </summary>
    public void CloseConnection()
    {
        if (_disposed) return;

        try
        {
            _cancellationTokenSource.Cancel();
            _textReader.Dispose();
            _textWriter.Dispose();
            Stream.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error closing connection: {ex}");
        }
        finally
        {
            _disposed = true;
        }
    }

   
    protected void OnTextReceived(string text)
    {
        TextReceived?.Invoke(this, new TextReceivedEventArgs(text));
    }

    public event EventHandler<TextReceivedEventArgs>? TextReceived;







    // ======================= TO-DO ====================================




    // public async Task ReadData()
    // {
    //     while (true)
    //     {
    //         var buffer = new Byte[256];
    //         var data = await Stream.ReadAsync(buffer);

    //     }
    // }


    // public void WriteData(Byte[] data)
    // {
    //     Stream.Write()
    // }


}