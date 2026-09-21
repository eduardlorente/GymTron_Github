using GymTron.Domain.Aggregates;
using GymTron.UnitTests.Helpers;

namespace GymTron.UnitTests.Domain;

public class ClockTests
{
    [Fact]
    public void FakeClock_DefaultInitialTime_IsUtc()
    {
        FakeClock clock = new();

        Assert.Equal(DateTimeKind.Utc, clock.UtcNow.Kind);
    }

    [Fact]
    public void FakeClock_SetTime_ReturnsCorrectUtcTime()
    {
        FakeClock clock = new();
        DateTime expectedTime = new(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        clock.SetTime(expectedTime);

        Assert.Equal(expectedTime, clock.UtcNow);
    }

    [Fact]
    public void FakeClock_SetLocalTime_ConvertsToUtc()
    {
        FakeClock clock = new();
        DateTime localTime = new(2026, 6, 15, 10, 30, 0, DateTimeKind.Local);

        clock.SetTime(localTime);

        Assert.Equal(DateTimeKind.Utc, clock.UtcNow.Kind);
        Assert.Equal(localTime.ToUniversalTime(), clock.UtcNow);
    }

    [Fact]
    public void FakeClock_Advance_AddsTimeToCurrentUtcNow()
    {
        FakeClock clock = new();
        DateTime initialTime = clock.UtcNow;

        clock.Advance(TimeSpan.FromHours(2));

        Assert.Equal(initialTime.AddHours(2), clock.UtcNow);
    }

    [Fact]
    public void FakeClock_Now_ReturnsLocalEquivalentOfUtcNow()
    {
        FakeClock clock = new();

        Assert.Equal(clock.UtcNow.ToLocalTime(), clock.Now);
    }

    [Fact]
    public void Training_WhenCreated_RecordsStartedOnFromClock()
    {
        FakeClock clock = new(new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc));

        Training training = Training.CreateAnStartedTraining(7, 3, [], clock);

        Assert.Equal(clock.UtcNow, training.StartedOn.FullDate);
    }

    [Fact]
    public void Training_WhenCompleted_RecordsCompletedOnFromClock()
    {
        FakeClock clock = new(new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc));
        Training training = Training.CreateAnStartedTraining(7, 3, [], clock);
        clock.Advance(TimeSpan.FromHours(1));

        training.Complete(clock);

        Assert.Equal(clock.UtcNow, training.CompletedOn!.FullDate);
    }
}
