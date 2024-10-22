using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.Clients;

public static class ChatServiceCollectionExtensions
{
    public static IServiceCollection AddChatServer(this IServiceCollection services, Action<ChatSettings> setupAction)
    {
        var settings = new ChatSettings();
        setupAction.Invoke(settings);

        services.AddSingleton(settings);
        services.AddSingleton<IChatServer, ChatServer>();

        return services;
    }
}