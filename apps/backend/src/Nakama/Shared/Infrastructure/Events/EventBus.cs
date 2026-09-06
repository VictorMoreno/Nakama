using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public sealed class EventBus(IPublisher publisher) : IEventBus
{
    public async Task Publish(IEnumerable<AggregateRoot> aggregates, CancellationToken ct)
    {
        foreach (var aggregate in aggregates)
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
    }

    /// <summary>
    /// Builds a <c>DomainEventNotification&lt;TEvent&gt;</c> for the runtime type of
    /// <paramref name="domainEvent"/> via reflection, since the aggregate's <c>DomainEvents</c>
    /// collection is typed as <see cref="IDomainEvent"/> and the concrete event type is only
    /// known at runtime.
    /// </summary>
    private static object WrapAsNotification(IDomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return Activator.CreateInstance(notificationType, domainEvent)!;
    }
}
