namespace GymTron.Domain.Events;

public interface IDomainEvent
{
    Guid CorrelationId { get; }
    DateTime OccurredOn { get; }
}
