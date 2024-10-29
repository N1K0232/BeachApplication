using BeachApplication.Clients.Settings;
using MessageBird;

namespace BeachApplication.Clients.PhoneNumber;

public class MessageSender(Client client, MessageSettings settings) : IMessageSender
{
    public Task SendAsync(string text, string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text, nameof(text));

        if (!long.TryParse(phoneNumber, out var recipient))
        {
            throw new ArgumentException("can't send the message", nameof(phoneNumber));
        }

        client.SendMessage(settings.PhoneNumber, text, [recipient]);
        return Task.CompletedTask;
    }
}