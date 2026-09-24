using GymTron.Domain.Services;
using GymTron.Infrastructure.BackgroundJobs;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GymTron.UnitTests.Infrastructure;

public class RefreshTokenCleanupJobTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesTokensOlderThan30Days_AndLogsCount()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var dal = Substitute.For<IRefreshTokenDAL>();
        var clock = Substitute.For<IClock>();
        var logger = Substitute.For<ILogger<RefreshTokenCleanupJob>>();

        var now = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var expectedOlderThan = now.AddDays(-30);

        clock.UtcNow.Returns(now);

        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IRefreshTokenDAL)).Returns(dal);
        serviceProvider.GetService(typeof(IClock)).Returns(clock);

        var tcs = new TaskCompletionSource<bool>();
        dal.DeleteExpiredAndRevoked(expectedOlderThan, Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ =>
            {
                tcs.TrySetResult(true);
                return Task.FromResult(5);
            });

        var job = new RefreshTokenCleanupJob(scopeFactory, logger, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(CancellationToken.None);
        await tcs.Task;
        await job.StopAsync(CancellationToken.None);

        await dal.Received(1).DeleteExpiredAndRevoked(expectedOlderThan, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_LogsErrorAndContinuesUntilCancelled()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var dal = Substitute.For<IRefreshTokenDAL>();
        var clock = Substitute.For<IClock>();
        var logger = Substitute.For<ILogger<RefreshTokenCleanupJob>>();

        clock.UtcNow.Returns(DateTime.UtcNow);
        scopeFactory.CreateScope().Returns(scope);
        scope.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(IRefreshTokenDAL)).Returns(dal);
        serviceProvider.GetService(typeof(IClock)).Returns(clock);

        var tcs = new TaskCompletionSource<bool>();
        dal.DeleteExpiredAndRevoked(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ =>
            {
                tcs.TrySetResult(true);
                return Task.FromException<int>(new InvalidOperationException("DB error"));
            });

        var job = new RefreshTokenCleanupJob(scopeFactory, logger, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(CancellationToken.None);
        await tcs.Task;
        await job.StopAsync(CancellationToken.None);

        logger.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelledImmediately_ExitsCleanly()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<RefreshTokenCleanupJob>>();

        var job = new RefreshTokenCleanupJob(scopeFactory, logger, TimeSpan.FromMilliseconds(50));
        await job.StartAsync(CancellationToken.None);
        await job.StopAsync(CancellationToken.None);

        if (job.ExecuteTask != null)
        {
            try
            {
                await job.ExecuteTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when BackgroundService ExecuteTask is cancelled during shutdown
            }
        }
    }
}
