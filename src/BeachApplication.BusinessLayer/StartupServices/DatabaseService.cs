using BeachApplication.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BeachApplication.BusinessLayer.StartupServices;

public class DatabaseService(IServiceProvider services, ILogger<DatabaseService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var dbCreator = context.GetService<IRelationalDatabaseCreator>();
            var strategy = context.Database.CreateExecutionStrategy();

            logger.LogInformation("Creating database");
            await strategy.ExecuteAsync(async () =>
            {
                var exists = await dbCreator.ExistsAsync(cancellationToken);
                if (!exists)
                {
                    await dbCreator.CreateAsync(cancellationToken);
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Caught an unexpected error while creating or migrating the database");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}