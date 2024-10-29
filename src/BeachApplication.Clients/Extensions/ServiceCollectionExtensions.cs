using BeachApplication.Clients.Chat;
using BeachApplication.Clients.Email;
using BeachApplication.Clients.PhoneNumber;
using BeachApplication.Clients.Settings;
using FluentEmail.Core.Interfaces;
using MessageBird;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.Clients.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatServer(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSettings<ChatSettings>(nameof(ChatSettings));

        services.AddSingleton(settings);
        services.AddSingleton<IChatServer, ChatServer>();

        return services;
    }

    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSettings<EmailSettings>(nameof(EmailSettings));

        services.AddSingleton(settings);
        services.AddFluentEmail(settings.EmailAddress).WithSendinblue();

        return services;
    }

    public static IServiceCollection AddMessageSender(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSettings<MessageSettings>(nameof(MessageSettings));
        services.AddSingleton(settings);

        services.AddScoped(_ => Client.CreateDefault(settings.ApiKey));
        services.AddScoped<IMessageSender, MessageSender>();

        return services;
    }

    private static T GetSettings<T>(this IConfiguration configuration, string sectionName) where T : class
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<T>();

        return settings;
    }

    private static FluentEmailServicesBuilder WithSendinblue(this FluentEmailServicesBuilder builder)
    {
        builder.Services.AddSingleton<ISender, EmailSender>();
        return builder;
    }
}