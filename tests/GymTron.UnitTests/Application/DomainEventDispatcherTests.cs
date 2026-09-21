using GymTron.Application.Common.Events;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Events;
using GymTron.UnitTests.Helpers;
using MediatR;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAndClearEvents_WhenEntityHasEvents_PublishesWrappedNotificationsAndClearsEvents()
    {
        Training training = Training.CreateAnStartedTraining(7, 1, [], new FakeClock());
        DomainEvent first = new(Guid.NewGuid(), new FakeClock());
        DomainEvent second = new(Guid.NewGuid(), new FakeClock());
        training.AddDomainEvent(first);
        training.AddDomainEvent(second);

        IPublisher publisher = Substitute.For<IPublisher>();
        MediatRDomainEventDispatcher dispatcher = new(publisher);

        await dispatcher.DispatchAndClearEvents(training, CancellationToken.None);

        Assert.Empty(training.DomainEvents);
        Received.InOrder(() =>
        {
            publisher.Publish(
                Arg.Is<DomainEventNotification<DomainEvent>>(n => n.DomainEvent == first),
                Arg.Any<CancellationToken>());
            publisher.Publish(
                Arg.Is<DomainEventNotification<DomainEvent>>(n => n.DomainEvent == second),
                Arg.Any<CancellationToken>());
        });
    }
}
