using GymTron.Domain.Enums;
using GymTron.Domain.Events;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public abstract class Entity<TId> : IHaveDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId? Id { get; protected set; }
    public EntityStatus Status { get; protected set; } = EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, default);
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Entity()
    {
    }

    public Entity(TId id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
