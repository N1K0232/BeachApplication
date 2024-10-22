using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BeachApplication.Clients;

public class ChatServer : IChatServer
{
    private readonly ChatSettings chatSettings;
    private readonly ILogger<ChatServer> logger;

    private Socket serverSocket = null;
    private Socket clientSocket = null;

    private Func<Socket, CancellationToken, Task> onClientAccepted;

    private bool isConnected = false;
    private bool disposed = false;

    public ChatServer(ChatSettings chatSettings, ILogger<ChatServer> logger)
    {
        this.chatSettings = chatSettings;
        this.logger = logger;
        Initialize();
    }

    public Func<Socket, CancellationToken, Task> OnClientAccepted
    {
        get
        {
            return onClientAccepted;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(OnClientAccepted));

            if (value != onClientAccepted)
            {
                onClientAccepted = value;
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!isConnected)
        {
            var hostName = Dns.GetHostName();
            var addresses = await Dns.GetHostAddressesAsync(hostName, cancellationToken);

            serverSocket.Bind(new IPEndPoint(addresses.FirstOrDefault(), chatSettings.Port));
            serverSocket.Listen(chatSettings.Backlog);
            isConnected = true;
        }
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!isConnected)
        {
            throw new SocketException(400, "Start the server before sending a message");
        }

        try
        {
            clientSocket = await serverSocket.AcceptAsync(cancellationToken);
            logger.LogInformation("Client connected at: {Date}", DateTime.UtcNow);

            await serverSocket.SendAsync(Encoding.ASCII.GetBytes("Client connected"));
            await onClientAccepted(clientSocket, cancellationToken);
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Couldn't connect to server");
        }
    }

    public Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!isConnected)
        {
            throw new SocketException(400, "Start the server before sending a message");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!disposed)
            {
                if (serverSocket is not null)
                {
                    if (serverSocket.Connected)
                    {
                        serverSocket.Close();
                    }

                    serverSocket.Dispose();
                    serverSocket = null;
                }

                disposed = true;
            }
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, GetType().FullName);

    private void Initialize()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
}