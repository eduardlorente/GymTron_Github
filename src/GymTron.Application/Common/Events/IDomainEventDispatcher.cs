using GymTron.Domain.Events;

namespace GymTron.Application.Common.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IHaveDomainEvents entity, CancellationToken cancellationToken = default);
}
