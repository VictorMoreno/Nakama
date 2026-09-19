using MediatR;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
