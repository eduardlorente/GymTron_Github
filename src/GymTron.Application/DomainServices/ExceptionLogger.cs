using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using Microsoft.Extensions.Logging;
using DomainLog = GymTron.Domain.Entities.Log;

namespace GymTron.Application.DomainServices;

internal class ExceptionLogger<T>(ILogger<T> logger, ILogRepository logRepository, IClock clock) : IExceptionLogger<T>
{
    private readonly ILogger<T> _logger = logger;
    private readonly ILogRepository _logRepository = logRepository;
    private readonly IClock _clock = clock;

    public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => _logger.Log(logLevel, eventId, state, exception, formatter);

    public async Task LogException(Exception exception)
        => await LogException(exception, string.Empty, string.Empty);

    public async Task LogException(Exception exception, string className, string methodName)
    {
        _logger.LogError(exception, $"Error in {className}.{methodName}");

        string message = GetExceptionMessage(exception);

        if (!string.IsNullOrWhiteSpace(className) && !string.IsNullOrWhiteSpace(methodName))
        {
            message = $"Error in {className}.{methodName}: {message}";
        }

        DomainLog log = DomainLog.New(message, _clock);

        await _logRepository.Add(log);
    }

    private static string GetExceptionMessage(Exception exception)
    {
        return exception.InnerException != null
            ? GetExceptionMessage(exception.InnerException)
            : exception.Message;
    }
}
