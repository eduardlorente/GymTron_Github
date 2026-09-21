using GymTron.Domain.Entities;

namespace GymTron.Domain.Aggregates;

public abstract class AggregateRoot<TId> : Entity<TId>
{


    public AggregateRoot()
        : base()
    { }
}
