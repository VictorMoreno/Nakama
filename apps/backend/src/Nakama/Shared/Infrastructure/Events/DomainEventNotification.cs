using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

/// <summary>
/// Wraps a raw <see cref="IDomainEvent"/> as a MediatR notification. Projections subscribe to
/// this generic wrapper (<c>INotificationHandler&lt;DomainEventNotification&lt;ChatbotCreated&gt;&gt;</c>)
/// instead of the domain event needing to implement <c>INotification</c> itself - that would leak
/// MediatR into the domain layer.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
