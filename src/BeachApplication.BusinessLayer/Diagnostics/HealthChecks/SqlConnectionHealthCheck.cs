using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BeachApplication.BusinessLayer.Diagnostics.HealthChecks;

public class SqlConnectionHealthCheck : IHealthCheck
{
    private readonly IConfiguration configuration;
    private readonly ILogger<SqlConnectionHealthCheck> logger;

    public SqlConnectionHealthCheck(IConfiguration configuration, ILogger<SqlConnectionHealthCheck> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("SqlConnection");
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(null, connection);

        try
        {
            await connection.OpenAsync(cancellationToken);
            command.CommandText = "SELECT 1";

            await command.ExecuteScalarAsync(cancellationToken);
            await connection.CloseAsync();

            logger.LogInformation("test connection succeeded");
            return HealthCheckResult.Healthy();
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "cannot execute");
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
        finally
        {
            logger.LogInformation("disposing resources");
            await command.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}