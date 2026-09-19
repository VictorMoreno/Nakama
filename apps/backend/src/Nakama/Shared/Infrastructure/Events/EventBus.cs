using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public sealed class EventBus(IPublisher publisher) : IEventBus
{
    public async Task Publish(AggregateRoot aggregate, CancellationToken ct)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            // IPublisher.Publish(object, CancellationToken) dispatches by the notification's
            // runtime type, which is exactly what's needed since DomainEvents is typed as
            // the IDomainEvent marker - the concrete DomainEventNotification<TEvent> is only
            // known at runtime.
            await publisher.Publish(WrapAsNotification(domainEvent), ct).ConfigureAwait(false);
        }

        aggregate.ClearDomainEvents();
    }

    private static object WrapAsNotification(IDomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return Activator.CreateInstance(notificationType, domainEvent)!;
    }
}
