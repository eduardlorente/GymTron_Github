namespace GymTron.Domain.Services;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}
