# ADR-12: `IQueryBus` for live cross-module reads, denormalized entities for per-event reads

- **Status:** Accepted
- **Area:** CQRS / cross-module reads

`IQueryBus.Ask<TResponse>` lets a class ask a query belonging to another bounded context and get back its `Response`,
without depending on that context's `Domain`/repository directly. Use `IQueryBus` when a class needs a value that
"belongs" to another module and an eventual-consistency copy isn't wanted; reach for a denormalized read-model entity
([ADR-9](0009-denormalized-read-model-entities-pattern.md)/[ADR-10](0010-cross-module-event-subscriptions-are-intentional-projection.md)) instead when the read happens on every event and a live cross-module call per event would be wasteful.

**Where it applies:** `Shared/Domain/{IQueryBus,IQuery}.cs`, `Shared/Infrastructure/Queries/QueryBus.cs`
