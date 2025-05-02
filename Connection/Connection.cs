using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TCPChatGUI.Connection;

/// <summary>
/// Contém o <see cref="NetworkStream"/> de uma conexão e implementa funções de comunicação entre cliente e servidor. 
/// </summary>
public partial class ChatConnection : ObservableObject
{
    [ObservableProperty]
    private NetworkStream _stream;

    [ObservableProperty]
    private IPEndPoint _connectionEndPoint;

    private readonly StreamReader _textReader;

    private readonly StreamWriter _textWriter;

    /// <summary>
    /// Token responsável por encerrar a leitura do stream.
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _disposed;


    public ChatConnection(NetworkStream stream, IPEndPoint endPoint)
    {
        Debug.WriteLine("Connection established.");
        Stream = stream;
        ConnectionEndPoint = endPoint;


        _textReader = new(Stream, Encoding.UTF8, leaveOpen: true);
        _textWriter = new(Stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

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
                TextReceived?.Invoke(this, new TextReceivedEventArgs(text.TrimEnd('\0')));
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Read operation canceled");
        }
        catch (IOException ex)
        {
            // Lançado quando o leitor não consegue mais ler (conexão fechada).
            Debug.WriteLine($"Network error: {ex.Message}");
            throw;
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


    public async Task ReadData()
    {
        var buffer = new Byte[256];

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (Stream == null || !Stream.CanRead) throw new InvalidOperationException("Stream não está disponível para leitura.");

            int bytesRead = await Stream.ReadAsync(buffer);

            if (bytesRead == 0) break;

            var receivedData = new byte[bytesRead];
            Array.Copy(buffer, receivedData, bytesRead);

            DataReceived?.Invoke(this, new DataReceivedEventArgs(receivedData));

        }
    }


    public async Task WriteData(Byte[] data)
    {
        if (Stream == null || !Stream.CanWrite) throw new InvalidOperationException("Stream não está disponível para escrita.");

        await Stream.WriteAsync(data);
        await Stream.FlushAsync();
    }


    /// <summary>
    /// Encerra a leitura do stream.
    /// </summary>
    public void StopReading()
    {
        _cancellationTokenSource.Cancel();
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

    public event EventHandler<TextReceivedEventArgs>? TextReceived;


    public event EventHandler<DataReceivedEventArgs>? DataReceived;



}

