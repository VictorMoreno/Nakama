# ADR-12b: Commands and queries never reference MediatR — `ICommandBus`/`IQueryBus` resolve handlers directly, no envelope

- **Status:** Accepted
- **Area:** CQRS / cross-module reads

Same intent as [ADR-7](0007-domain-events-never-reference-mediatr.md) (domain events never reference MediatR), extended to the request/response side: no `Command`/
`Query` record implements MediatR's `IRequest<TResponse>`, and no `Handler` implements `IRequestHandler<,>`. Instead,
every command implements the framework-agnostic `ICommand` (no `TResponse` — every command in this codebase is a pure
side effect, so there is nothing to return) and its handler implements `ICommandHandler<TCommand>`; every query
implements `IQuery<TResponse>` (unchanged) and its handler implements `IQueryHandler<TQuery, TResponse>`. `CommandBus`/
`QueryBus` are the only two classes that know MediatR/DI exist: `ICommandBus.Send`/`IQueryBus.Ask` resolve the closed
`ICommandHandler<TCommand>`/`IQueryHandler<TQuery, TResponse>` directly from the DI container via `MakeGenericType`
(same reflection technique as `EventBus.WrapAsNotification`, since the concrete command/query type is only known at
runtime) and invoke `Handle` on it.

This deliberately does **not** route through MediatR's `ISender`, unlike `IEventBus`. An earlier version of this design
tried an envelope pair (`CommandEnvelope<TCommand> : IRequest`, dispatched by an open-generic
`CommandEnvelopeHandler<TCommand> : IRequestHandler<CommandEnvelope<TCommand>>`) mirroring `DomainEventNotification`/
`DomainEventHandler` exactly — that compiles, but fails at runtime: .NET's built-in DI only resolves an open-generic
registration by substituting generic parameters *positionally*, so
`services.AddTransient(typeof(IRequestHandler<>), typeof(CommandEnvelopeHandler<>))` tries to construct
`CommandEnvelopeHandler<CommandEnvelope<TCommand>>` (plugging the *entire requested generic argument* back in) instead
of solving for `TCommand` — it does not unify through the wrapper type the way the domain-event side implicitly assumed.
The domain-event side avoids this because `DomainEventHandler<TEvent>` is never registered as an open generic itself —
every *concrete* handler (`ChatbotSummaryUpdatedOnChatbotDocumentAdded`, etc.) is a non-generic class closing `TEvent`,
which MediatR's assembly scan finds one at a time like any ordinary handler. Commands/queries have no such per-type
concrete subclass, so the open-generic MediatR route was a dead end here — verified by an actual DI container resolution
at runtime, not just a build. Direct `IServiceProvider` resolution sidesteps the whole problem.

**DI registration:** `services.Scan(...)` in `InfrastructureExtensions.cs` registers every `ICommandHandler<>`/
`IQueryHandler<,>` implementation found in the Nakama assembly (mirrors the existing `IEndpoint` Scrutor scan) —
`AddMediatR`'s own scan only ever covers the domain-event side.

**Where it applies:** `Shared/Domain/{ICommand,ICommandHandler,ICommandBus,IQueryHandler}.cs`,
`Shared/Infrastructure/Commands/CommandBus.cs`, `Shared/Infrastructure/Queries/QueryBus.cs`,
`apps/backend/api/Configuration/InfrastructureExtensions.cs`, every `Command`/`Query`/`Handler` in `**/Application/**`
