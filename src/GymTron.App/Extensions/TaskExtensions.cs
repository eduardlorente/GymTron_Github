using Microsoft.Extensions.Logging;

namespace GymTron.App.Extensions;

public static class TaskExtensions
{
    public static void SafeFireAndForget(
        this Task task,
        Action<Exception>? onException = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.IsCompletedSuccessfully)
        {
            return;
        }

        _ = HandleAsync(task, onException, logger);
    }

    private static async Task HandleAsync(
        Task task,
        Action<Exception>? onException,
        ILogger? logger)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An unhandled exception occurred in a fire-and-forget task: {Message}", ex.Message);
            onException?.Invoke(ex);
        }
    }
}
