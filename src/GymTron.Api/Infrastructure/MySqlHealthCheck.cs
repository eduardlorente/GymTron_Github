using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace GymTron.Api.Infrastructure;

public class MySqlHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("MySQL database connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MySQL database connection failed.", ex);
        }
    }
}
