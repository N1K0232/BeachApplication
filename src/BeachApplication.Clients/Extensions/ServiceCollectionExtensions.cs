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
        var settings = services.ConfigureAndGet<ChatSettings>(configuration, nameof(ChatSettings));
        services.AddSingleton(settings);

        services.AddSingleton<IChatServer, ChatServer>();
        return services;
    }

    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = services.ConfigureAndGet<EmailSettings>(configuration, nameof(EmailSettings));
        services.AddFluentEmail(settings.EmailAddress).WithSendinblue();

        return services;
    }

    public static IServiceCollection AddMessageSender(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = services.ConfigureAndGet<MessageSettings>(configuration, nameof(MessageSettings));
        services.AddSingleton(settings);

        services.AddScoped(_ => Client.CreateDefault(settings.ApiKey));
        services.AddScoped<IMessageSender, MessageSender>();

        return services;
    }

    private static T ConfigureAndGet<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<T>();

        services.Configure<T>(section);
        return settings;
    }

    private static FluentEmailServicesBuilder WithSendinblue(this FluentEmailServicesBuilder builder)
    {
        builder.Services.AddSingleton<ISender, EmailSender>();
        return builder;
    }
}