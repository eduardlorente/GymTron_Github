using GymTron.Domain.Services;

namespace GymTron.UnitTests.Helpers;

public class FakeClock : IClock
{
    private DateTime _currentTime;

    public FakeClock(DateTime? initialTime = null)
    {
        _currentTime = initialTime ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }

    public DateTime UtcNow => _currentTime;
    public DateTime Now => _currentTime.ToLocalTime();

    public void SetTime(DateTime time)
    {
        _currentTime = time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
    }

    public void Advance(TimeSpan duration)
    {
        _currentTime = _currentTime.Add(duration);
    }
}
