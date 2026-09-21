using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class BodyWeight : Entity<int>
{
    public int? UserId { get; private set; }
    public decimal Weight { get; private set; } = 0;
    public decimal BodyFatPercentage { get; private set; } = 0;

    private BodyWeight(int id,
                       int? userId,
                       decimal weight,
                       decimal bodyFatPercentage,
                       DateTime createdOn)
        : base(id)
    {
        UserId = userId;
        Weight = weight;
        BodyFatPercentage = bodyFatPercentage;
        Status = EntityStatus.FromDatabase(createdOn);
    }

    public static BodyWeight New(decimal weight,
                                 decimal bodyFatPercentage,
                                 IClock clock,
                                 int? userId = null)
    {
        return new BodyWeight(0, userId, weight, bodyFatPercentage, clock.UtcNow);
    }

    public static BodyWeight New(decimal weight,
                                 decimal bodyFatPercentage,
                                 DateTime createdOn,
                                 int? userId = null)
    {
        return new BodyWeight(0, userId, weight, bodyFatPercentage, createdOn);
    }

    public static BodyWeight FromDatabase(int id,
                                          decimal weight,
                                          decimal bodyFatPercentage,
                                          DateTime createdOn,
                                          int? userId = null)
    {
        return new BodyWeight(id, userId, weight, bodyFatPercentage, createdOn);
    }
}
