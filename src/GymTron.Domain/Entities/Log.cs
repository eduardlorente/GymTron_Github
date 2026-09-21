using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class Log : Entity<int>
{


    public string Message { get; private set; } = string.Empty;


    private Log(string message, DateTime createdOn)
        : base(0)
    {
        Message = message;
        Status = EntityStatus.FromDatabase(createdOn);
    }


    public static Log New(string message, IClock clock)
    {
        return new Log(message, clock.UtcNow);
    }


    public static Log New(string message, DateTime createdOn)
    {
        return new Log(message, createdOn);
    }


    public static Log New(string message)
    {
        return new Log(message, default);
    }
}
