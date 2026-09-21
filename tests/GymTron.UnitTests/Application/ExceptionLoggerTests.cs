using GymTron.Application.DomainServices;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class ExceptionLoggerTests
{
    [Fact]
    public async Task LogException_WithNestedException_SendsDeepestMessage()
    {
        ILogger<ExceptionLoggerTests> logger = Substitute.For<ILogger<ExceptionLoggerTests>>();
        ILogRepository logRepository = Substitute.For<ILogRepository>();
        IClock clock = Substitute.For<IClock>();
        ExceptionLogger<ExceptionLoggerTests> sut = new(logger, logRepository, clock);

        await sut.LogException(new InvalidOperationException("outer", new Exception("root")));

        await logRepository.Received(1).Add(
            Arg.Is<Log>(log => log.Message == "root"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogException_WithLocation_PrefixesPersistedMessage()
    {
        ILogger<ExceptionLoggerTests> logger = Substitute.For<ILogger<ExceptionLoggerTests>>();
        ILogRepository logRepository = Substitute.For<ILogRepository>();
        IClock clock = Substitute.For<IClock>();
        ExceptionLogger<ExceptionLoggerTests> sut = new(logger, logRepository, clock);

        await sut.LogException(new Exception("failure"), "TrainingHandler", "Handle");

        await logRepository.Received(1).Add(
            Arg.Is<Log>(log => log.Message == "Error in TrainingHandler.Handle: failure"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void LoggerMembers_DelegateToUnderlyingLogger()
    {
        ILogger<ExceptionLoggerTests> logger = Substitute.For<ILogger<ExceptionLoggerTests>>();
        ILogRepository logRepository = Substitute.For<ILogRepository>();
        IClock clock = Substitute.For<IClock>();
        IDisposable scope = Substitute.For<IDisposable>();
        logger.BeginScope("scope").Returns(scope);
        logger.IsEnabled(LogLevel.Warning).Returns(true);
        ExceptionLogger<ExceptionLoggerTests> sut = new(logger, logRepository, clock);
        Func<string, Exception?, string> formatter = (state, _) => state;
        Exception exception = new("failure");

        Assert.Same(scope, sut.BeginScope("scope"));
        Assert.True(sut.IsEnabled(LogLevel.Warning));
        sut.Log(LogLevel.Error, new EventId(7), "state", exception, formatter);

        logger.Received(1).Log(LogLevel.Error, new EventId(7), "state", exception, formatter);
    }
}
