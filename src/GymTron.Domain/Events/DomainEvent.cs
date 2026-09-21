using GymTron.Domain.Services;

namespace GymTron.Domain.Events;

public class DomainEvent : IDomainEvent
{
    public Guid CorrelationId { get; private set; }
    public DateTime OccurredOn { get; private set; }

    public DomainEvent(Guid correlationId, IClock clock)
        : this(correlationId, clock.UtcNow)
    {
    }

    public DomainEvent(Guid correlationId, DateTime occurredOn)
    {
        CorrelationId = correlationId;
        OccurredOn = occurredOn;
    }
}
