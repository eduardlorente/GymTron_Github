using GymTron.Api.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace GymTron.UnitTests.Infrastructure;

public class MySqlHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithUnreachableServer_ReturnsUnhealthyResult()
    {
        var healthCheck = new MySqlHealthCheck("Server=127.0.0.1;Port=65534;Database=nonexistent;Uid=root;Pwd=wrong;Connection Timeout=1;");
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("MySQL database connection failed.", result.Description);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancelled_ReturnsUnhealthyResult()
    {
        var healthCheck = new MySqlHealthCheck("Server=127.0.0.1;Port=65534;Database=nonexistent;Uid=root;Pwd=wrong;Connection Timeout=1;");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("MySQL database connection failed.", result.Description);
    }
}
