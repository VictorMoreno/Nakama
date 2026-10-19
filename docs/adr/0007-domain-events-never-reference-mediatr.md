# ADR-7: Domain events never reference MediatR

- **Status:** Accepted
- **Area:** Domain events

`IDomainEvent` is intentionally free of any MediatR/infrastructure reference — the domain only knows it raised a fact,
never how it gets dispatched. `DomainEventNotification<TEvent>` wraps a raw `IDomainEvent` as a MediatR `INotification`
so projections can subscribe to `INotificationHandler<DomainEventNotification<TEvent>>` instead of the domain event
needing to implement `INotification` itself, which would leak MediatR into the domain layer. `EventBus.Publish` builds
the wrapper via reflection (`typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType())`) since the
aggregate's `DomainEvents` collection is typed as the `IDomainEvent` marker and the concrete event type is only known at
runtime. `DomainEventHandler<TEvent>` is the adapter base every concrete handler now inherits: it implements
`INotificationHandler<DomainEventNotification<TEvent>>` once and unwraps the envelope before calling into the concrete
handler's own `Handle(TEvent, ct)` — so MediatR stays fully infrastructure-side and every handler's own signature takes
the raw event.

**Where it applies:** `Shared/Domain/IDomainEvent.cs`,
`Shared/Infrastructure/Events/{DomainEventNotification,DomainEventHandler,EventBus}.cs`, every `*On*` handler under
`**/Application/Handlers/`
