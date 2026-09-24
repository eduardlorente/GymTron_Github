using GymTron.Domain.Services;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymTron.Infrastructure.BackgroundJobs;

public class RefreshTokenCleanupJob(
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupJob> logger,
    TimeSpan? checkInterval = null) : BackgroundService
{
    private readonly TimeSpan _checkInterval = checkInterval ?? TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dal = scope.ServiceProvider.GetRequiredService<IRefreshTokenDAL>();
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();

                // Purge tokens older than 30 days
                int deleted = await dal.DeleteExpiredAndRevoked(clock.UtcNow.AddDays(-30), stoppingToken);
                if (deleted > 0)
                {
                    logger.LogInformation("Deleted {Count} expired/revoked refresh tokens.", deleted);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during refresh token cleanup.");
            }
        }
    }
}
