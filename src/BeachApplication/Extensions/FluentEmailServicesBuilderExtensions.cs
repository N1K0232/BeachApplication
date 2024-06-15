using BeachApplication.BusinessLayer.Email;
using FluentEmail.Core.Interfaces;

namespace BeachApplication.Extensions;

public static class FluentEmailServicesBuilderExtensions
{
    public static FluentEmailServicesBuilder WithSendinblue(this FluentEmailServicesBuilder builder)
    {
        builder.Services.AddSingleton<ISender, SendinblueSender>();
        return builder;
    }
}