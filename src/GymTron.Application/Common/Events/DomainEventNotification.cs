using GymTron.Domain.Events;
using MediatR;

namespace GymTron.Application.Common.Events;

public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
