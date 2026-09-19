using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public abstract class DomainEventHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent
{
    public Task Handle(DomainEventNotification<TEvent> notification, CancellationToken ct) =>
        this.Handle(notification.DomainEvent, ct);

    protected abstract Task Handle(TEvent domainEvent, CancellationToken ct);
}
