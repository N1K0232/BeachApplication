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
        using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

        logger.LogInformation("Running migrations");
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Database.MigrateAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            await transaction.DisposeAsync();
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}