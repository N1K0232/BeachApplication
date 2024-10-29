using System.Net.Sockets;

namespace BeachApplication.Clients.Chat;

public interface IChatServer : IDisposable
{
    Func<Socket, CancellationToken, Task> OnClientAccepted { get; set; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task SendAsync(string message, CancellationToken cancellationToken = default);
}