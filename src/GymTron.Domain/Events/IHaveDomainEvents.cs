namespace GymTron.Domain.Events;

public interface IHaveDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
