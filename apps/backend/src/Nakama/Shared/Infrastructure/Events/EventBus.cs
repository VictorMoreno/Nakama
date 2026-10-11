using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public sealed class EventBus(IPublisher publisher) : IEventBus
{
    public async Task Publish(AggregateRoot aggregate, CancellationToken ct)
    {
        foreach (IDomainEvent domainEvent in aggregate.DomainEvents)
        {
            await publisher.Publish(WrapAsNotification(domainEvent), ct).ConfigureAwait(false);
        }

        aggregate.ClearDomainEvents();
    }

    private static object WrapAsNotification(IDomainEvent domainEvent)
    {
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return Activator.CreateInstance(notificationType, domainEvent)!;
    }
}
