using Microsoft.Extensions.Logging;

namespace GymTron.Domain.Services;

public interface IExceptionLogger<T> : ILogger<T>
{
    Task LogException(Exception exception);
    Task LogException(Exception exception, string className, string methodName);
}
