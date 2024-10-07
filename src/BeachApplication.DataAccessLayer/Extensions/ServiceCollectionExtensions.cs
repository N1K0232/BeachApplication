using BeachApplication.DataAccessLayer.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.DataAccessLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerContext(this IServiceCollection services, string connectionString)
    {
        services.AddSqlServer<ApplicationDbContext>(connectionString, options =>
        {
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            options.CommandTimeout(120);
            options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        });

        services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static IServiceCollection AddSqlServerCaching(this IServiceCollection services, string connectionString)
    {
        services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = connectionString;
            options.TableName = "CacheStore";
            options.SchemaName = "dbo";
            options.DefaultSlidingExpiration = TimeSpan.FromDays(1);
        });

        services.AddSingleton<ISqlClientCache, SqlClientCache>();
        return services;
    }
}