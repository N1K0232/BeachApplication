using BeachApplication.DataAccessLayer.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.DataAccessLayer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlContext(this IServiceCollection services, Action<SqlContextOptions> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var options = new SqlContextOptions();
        configuration.Invoke(options);

        services.AddScoped(_ => options);
        services.AddScoped<ISqlContext, SqlContext>();

        return services;
    }
}