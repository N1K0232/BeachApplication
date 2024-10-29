namespace BeachApplication.Clients.PhoneNumber;

public interface IMessageSender
{
    Task SendAsync(string text, string phoneNumber);
}