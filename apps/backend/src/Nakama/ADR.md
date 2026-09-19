# Architecture Decision Records — Nakama backend

Non-obvious rationale that used to live as XML `<summary>` comments across `Domain`/`Application`/`Infrastructure`. Code outside `apps/backend/api` carries no doc comments by convention — it must be self-descriptive through naming/structure. Anything that isn't obvious from the code itself lives here instead. `agents.md` (in this same folder) documents *how* to write code here; this file documents *why* certain decisions were made.

## Persistence

### ADR-1: Session factories are lazy, shared singletons
`IWriteSessionFactory`/`IReadSessionFactory` (both shaped by `ISessionFactory`) wrap an NHibernate `ISessionFactory`. Registered as singletons, built lazily on first use rather than at DI-container-build time — building one is expensive, and lazy build lets the app start and compile/run without a reachable Postgres instance. `ISessionFactory` is the shared shape so `NhRepositoryBase<TEntity, TId>` can accept either side without depending on the write/read-specific DI token. `NhSessionFactoryBuilder` holds the actual build logic (Postgres dialect, connection string from `ConnectionStrings:<name>`); database-per-tenant resolution is explicitly out of scope for the current pass, left as a placeholder for later.

**Where it applies:** `Shared/Infrastructure/Persistence/{IWriteSessionFactory,IReadSessionFactory,ISessionFactory,NhWriteSessionFactory,NhReadSessionFactory,NhSessionFactoryBuilder}.cs`

### ADR-2: `NhRepositoryBase` — shared CRUD via inheritance, not delegation
`FindById`, `Save`, and `Update` are `public` on the base (not `protected`) precisely so a concrete `I...Repository` whose identity lookup is a plain primary-key fetch can declare `FindById(TId, CancellationToken)` with that exact name and signature and have it satisfied purely by inheriting from `NhRepositoryBase<TEntity, TId>` — no per-repo delegator method needed. `Update` is a pure alias to `Save` (NHibernate's `SaveOrUpdateAsync` already handles both insert and update transparently); it exists as a separate name purely so call sites can express intent (creating vs. updating an existing row). A repo whose lookup needs extra shaping — e.g. a singleton with no caller-supplied id (`NhAnalyticsRepository.Find`) — still wraps `FindById` in its own one-liner, since that can't be satisfied by inheritance alone.

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/NhRepositoryBase.cs` and every `I...Repository`/`Nh...Repository` pair.

### ADR-3: Generic value-object persistence via reflection
Identity value objects are positional record structs implementing either `IStringValueObject` (`Value: string`) or `IGuidValueObject` (`Value: Guid`). `ValueObjectUserType<T>`/`GuidValueObjectUserType<T>` (siblings) map any such type generically onto a varchar/native-uuid column respectively, round-tripping through the type's single-argument constructor via `Activator.CreateInstance` — this avoids writing a bespoke NHibernate `IUserType` per identity type.

**Where it applies:** `Shared/Domain/{IStringValueObject,IGuidValueObject}.cs`, `Shared/Infrastructure/Persistence/Nh/UserTypes/{ValueObjectUserType,GuidValueObjectUserType}.cs`

### ADR-4: Complex nested values persist as a single JSON column
`JsonUserType<T>` persists any complex .NET value (a list, a record, a nested value-object graph — e.g. the quiz embedded in a formation module, or a chatbot's documents/team lists) as one JSON text column, per agents.md §6. This avoids modelling every nested collection as its own NHibernate component/bag mapping. Used for `Chatbot.Settings` and the private `_documents`/`_team` backing fields (`ChatbotMapping`).

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/UserTypes/JsonUserType.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/ChatbotMapping.cs`

### ADR-5: Cross-aggregate/cross-module references are raw identifiers, never the other side's Domain type
`Formation.LinkedChatbotKey` is a plain `Guid`, never `Chatbots.Domain.ChatbotId` — a bounded context must not reference another context's Domain (agents.md §1). Same rule for `Conversation`, which references `Chatbot` only by `ChatbotId` value, never by object reference or NHibernate association — it's deliberately its own aggregate (see ADR-9 below for why). Mapping-wise, `FormationMapping`/`ConversationMapping` declare these as plain scalar columns, never NHibernate associations, for the same reason.

**Where it applies:** `Formations/Domain/Formation.cs`, `Chatbots/Domain/Conversation.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/{FormationMapping,ConversationMapping}.cs`

### ADR-6: Protected parameterless constructors exist only for NHibernate proxy hydration
`Chatbot()`, `Conversation()`, `Formation()` (and similar) have a `protected`/empty constructor that is never meant to be called by hand — it exists solely so NHibernate can generate lazy-load proxies and hydrate instances via reflection. Same reasoning behind `AggregateRoot<TId>.Id` using `virtual` + a private setter: `virtual` is what NHibernate needs for proxies, the private setter keeps mutation confined to the aggregate's own behavior methods, and NHibernate still reaches it via reflection when hydrating.

**Where it applies:** every aggregate's parameterless constructor; `Shared/Domain/AggregateRoot.cs`

## Domain events

### ADR-7: Domain events never reference MediatR
`IDomainEvent` is intentionally free of any MediatR/infrastructure reference — the domain only knows it raised a fact, never how it gets dispatched. `DomainEventNotification<TEvent>` wraps a raw `IDomainEvent` as a MediatR `INotification` so projections can subscribe to `INotificationHandler<DomainEventNotification<TEvent>>` instead of the domain event needing to implement `INotification` itself, which would leak MediatR into the domain layer. `EventBus.Publish` builds the wrapper via reflection (`typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType())`) since the aggregate's `DomainEvents` collection is typed as the `IDomainEvent` marker and the concrete event type is only known at runtime. `DomainEventHandler<TEvent>` is the adapter base every concrete handler now inherits: it implements `INotificationHandler<DomainEventNotification<TEvent>>` once and unwraps the envelope before calling into the concrete handler's own `Handle(TEvent, ct)` — so MediatR stays fully infrastructure-side and every handler's own signature takes the raw event.

**Where it applies:** `Shared/Domain/IDomainEvent.cs`, `Shared/Infrastructure/Events/{DomainEventNotification,DomainEventHandler,EventBus}.cs`, every `*On*` handler under `**/Application/Handlers/`

### ADR-8: Event dispatch is synchronous, no outbox
`IEventBus.Publish` takes a single aggregate (every use-case Handler in this codebase loads, mutates, and saves exactly one aggregate per transaction — see ADR-5, cross-aggregate work happens via events/read-models, never a shared transaction) and publishes every pending domain event it raised, then clears them. It's invoked explicitly at the end of each use-case Handler, after the write-side transaction has committed — NHibernate has no `SaveChangesInterceptor`-style hook, and this scope has no outbox, so dispatch is synchronous within the same request. A crash between commit and publish would lose the event; acceptable for the current scope, worth revisiting if delivery guarantees matter later.

**Where it applies:** `Shared/Infrastructure/Events/{IEventBus,EventBus}.cs`

### ADR-9: Denormalized read-model entities pattern
A read entity (`ChatbotSummary`, `ChatbotDetail`, `FormationSummary`, `FormationDetail`, `ChatPage`, `Analytics`) implements `IEntity` like any domain entity but is **not** an `AggregateRoot`: it never raises its own events, it only consumes events raised by a real aggregate, and it's fed exclusively through its own module's `Application/Handlers`. One read model exists per route, not per tab/section — e.g. `ChatbotDetail` covers all 4 tabs (General, Documents, Team, Analytics) of `/chatbots/:key` in one entity, including a cross-aggregate derivation (`Apply(ConversationExchangeAppended)` computes the Analytics tab's "most repeated questions" from `Conversation` events). Where no real signal exists yet for a field (e.g. `ChatbotDetail.Gaps`, "was this answer good"), it's deliberately left empty rather than faking a heuristic. `ChatPage` is the clearest cross-aggregate case: fed by both `Chatbot` (label/settings) and `Conversation` (history) events without either aggregate referencing the other's Domain type. `Analytics` is the degenerate case: not an aggregate, no identity of its own, a singleton row (`SingletonKey` is always `"global"`) computed entirely as a derived projection across `Chatbot`/`Conversation`/`Formation` events — see ADR-11 for its specific defensive-handler shape.

**Where it applies:** `ChatbotsList/Domain/ChatbotSummary.cs`, `ChatbotDetail/Domain/ChatbotDetail.cs`, `FormationsList/Domain/FormationSummary.cs`, `FormationDetail/Domain/FormationDetail.cs`, `ChatPage/Domain/ChatPage.cs`, `Analytics/Domain/Analytics.cs`

### ADR-10: Cross-module event subscriptions are intentional projection coupling
`ChatbotLabelRef` is `FormationDetail`'s own local denormalized copy of a chatbot's label, keyed by the chatbot's id. It is **not** exposed through `IFormationDetailRepository`/Application — it exists purely so `FormationDetail`'s own projection handler can resolve `Chatbot.Label` for the "Basado en la documentación de X" caption without `FormationDetail`'s Infrastructure ever touching `Chatbots`' Domain entities or its read model's repository directly. `ChatbotLabelRefCreatedOnChatbotCreated` listens to `ChatbotCreated` — a totally different module's `Domain/Events` — which is the intentional projection coupling this pattern relies on. There is no `ChatbotLabelChanged` event (a label never changes after creation), so `ChatbotLabelRef` has no other `Apply` method. `FormationDetail.LinkedChatbotLabel` is resolved at creation time (`FormationDetailCreatedOnFormationCreated`) via this local lookup, kept current by the handler above, so the caption never needs a live cross-context join.

**Where it applies:** `FormationDetail/Domain/ChatbotLabelRef.cs`, `FormationDetail/Domain/IChatbotLabelRefRepository.cs`, `FormationDetail/Application/Handlers/ChatbotLabelRefCreatedOnChatbotCreated.cs`, `FormationDetail/Application/Handlers/FormationDetailCreatedOnFormationCreated.cs`, `FormationDetail/Domain/FormationDetail.cs`

### ADR-11: Analytics defensive dual-path handler
`Analytics` has no dedicated creation event of its own — it's fed by whichever of several event types (`ChatbotCreated`, `ConversationExchangeAppended`, ...) arrives first. Its handlers are therefore defensive: on the first feeding event to arrive, create the row via a `CreateFromX` factory and `Save`; if the row already exists (some other feeding event got there first), apply the event onto it and `Update` instead.

**Where it applies:** `Analytics/Application/Handlers/{AnalyticsCreatedOnChatbotCreated,AnalyticsCreatedOnConversationExchangeAppended}.cs`

## CQRS / cross-module reads

### ADR-12: `IQueryBus` for live cross-module reads, denormalized entities for per-event reads
`IQueryBus.Ask<TResponse>` lets a class ask a query belonging to another bounded context and get back its `Response`, without depending on that context's `Domain`/repository directly. Use `IQueryBus` when a class needs a value that "belongs" to another module and an eventual-consistency copy isn't wanted; reach for a denormalized read-model entity (ADR-9/ADR-10) instead when the read happens on every event and a live cross-module call per event would be wasteful.

**Where it applies:** `Shared/Domain/{IQueryBus,IQuery}.cs`, `Shared/Infrastructure/Queries/QueryBus.cs`

### ADR-12b: Commands and queries never reference MediatR — `ICommandBus`/`IQueryBus` resolve handlers directly, no envelope
Same intent as ADR-7 (domain events never reference MediatR), extended to the request/response side: no `Command`/`Query` record implements MediatR's `IRequest<TResponse>`, and no `Handler` implements `IRequestHandler<,>`. Instead, every command implements the framework-agnostic `ICommand` (no `TResponse` — every command in this codebase is a pure side effect, so there is nothing to return) and its handler implements `ICommandHandler<TCommand>`; every query implements `IQuery<TResponse>` (unchanged) and its handler implements `IQueryHandler<TQuery, TResponse>`. `CommandBus`/`QueryBus` are the only two classes that know MediatR/DI exist: `ICommandBus.Send`/`IQueryBus.Ask` resolve the closed `ICommandHandler<TCommand>`/`IQueryHandler<TQuery, TResponse>` directly from the DI container via `MakeGenericType` (same reflection technique as `EventBus.WrapAsNotification`, since the concrete command/query type is only known at runtime) and invoke `Handle` on it.

This deliberately does **not** route through MediatR's `ISender`, unlike `IEventBus`. An earlier version of this design tried an envelope pair (`CommandEnvelope<TCommand> : IRequest`, dispatched by an open-generic `CommandEnvelopeHandler<TCommand> : IRequestHandler<CommandEnvelope<TCommand>>`) mirroring `DomainEventNotification`/`DomainEventHandler` exactly — that compiles, but fails at runtime: .NET's built-in DI only resolves an open-generic registration by substituting generic parameters *positionally*, so `services.AddTransient(typeof(IRequestHandler<>), typeof(CommandEnvelopeHandler<>))` tries to construct `CommandEnvelopeHandler<CommandEnvelope<TCommand>>` (plugging the *entire requested generic argument* back in) instead of solving for `TCommand` — it does not unify through the wrapper type the way the domain-event side implicitly assumed. The domain-event side avoids this because `DomainEventHandler<TEvent>` is never registered as an open generic itself — every *concrete* handler (`ChatbotSummaryUpdatedOnChatbotDocumentAdded`, etc.) is a non-generic class closing `TEvent`, which MediatR's assembly scan finds one at a time like any ordinary handler. Commands/queries have no such per-type concrete subclass, so the open-generic MediatR route was a dead end here — verified by an actual DI container resolution at runtime, not just a build. Direct `IServiceProvider` resolution sidesteps the whole problem.

**DI registration:** `services.Scan(...)` in `InfrastructureExtensions.cs` registers every `ICommandHandler<>`/`IQueryHandler<,>` implementation found in the Nakama assembly (mirrors the existing `IEndpoint` Scrutor scan) — `AddMediatR`'s own scan only ever covers the domain-event side.

**Where it applies:** `Shared/Domain/{ICommand,ICommandHandler,ICommandBus,IQueryHandler}.cs`, `Shared/Infrastructure/Commands/CommandBus.cs`, `Shared/Infrastructure/Queries/QueryBus.cs`, `apps/backend/api/Configuration/InfrastructureExtensions.cs`, every `Command`/`Query`/`Handler` in `**/Application/**`

## Domain model notes

### ADR-13: Deterministic infrastructure seams instead of static calls
Factory methods take `ISystemClock` instead of calling `DateTime.UtcNow` directly, so domain behavior (timestamps) stays deterministic and testable via injection.

**Where it applies:** `Shared/Domain/ISystemClock.cs`, every aggregate factory method that stamps a time (e.g. `Chatbot.Create`)

### ADR-13b: Ids for newly-created aggregates/sub-entities are caller-supplied, not server-generated
`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`, and `ConversationExchangeAppender` (when starting a new conversation) used to generate their new entity's id server-side via `IUuidGenerator` and return it in a `Response`, which was the only reason those 4 commands returned anything — every other command in the codebase already returns an empty `Response`. Since a client-generated GUID v4 has negligible collision probability, the id is now supplied by the caller instead: the client already knows the id before sending the command, so the command can stay a pure side effect with an empty `Response`, and `IUuidGenerator`/`GuidUuidGenerator` were deleted as dead code (they had no other callers). This does introduce a risk that server-generated ids never had: a client could resend or accidentally reuse an id, and `Save`'s NHibernate `SaveOrUpdate` semantics would silently overwrite the existing row instead of failing. The 3 pure-create handlers (`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`) guard against this explicitly with a `FindById`-then-throw-`AlreadyExists` check before creating. `ConversationExchangeAppender` is the one exception: finding an existing row for the supplied id is its *intended* "continue this conversation" path, not a collision, so it has no such guard — it's a legitimate find-or-start.

**Where it applies:** `Chatbots/Application/Creator/ChatbotCreator.cs`, `Formations/Application/Creator/FormationCreator.cs`, `Formations/Application/ModuleAdder/FormationModuleAdder.cs`, `Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Chatbots/Domain/Errors/ChatbotErrors.cs`, `Formations/Domain/Errors/FormationErrors.cs`

### ADR-14: `DomainError` doubles as the exception it throws
There is no separate wrapper exception — a concrete error (e.g. `ChatbotErrors.OwnerCannotBeRemoved`) is thrown directly (`throw ChatbotErrors.OwnerCannotBeRemoved;`). Concrete errors live in each module's `Domain/Errors` folder with a stable, project-prefixed code (`nakama.<module>...`).

**Where it applies:** `Shared/Domain/DomainError.cs` and every module's `Domain/Errors/*`

### ADR-15: Several Domain enums intentionally mirror a frontend TypeScript union
`ModuleType`, `FormationStatus`, `ChatbotStatus`, `LlmProvider`, `DocStatus` each mirror an equivalent frontend union type, kept in sync by convention (not shared code) since these are two separate stacks. `TeamRole` is a partial mirror: the frontend distinguishes grammatical gender for the owner label ("Propietario"/"Propietaria") for display purposes only — the domain models a single `Owner` role, rendering the gendered label is a presentation concern. `TeamMember`'s `isOwnerRole`-equivalent check is the domain invariant carrier behind that same frontend concept.

**Where it applies:** `Chatbots/Domain/{ChatbotStatus,LlmProvider,DocStatus,TeamRole,TeamMember}.cs`, `Formations/Domain/{FormationStatus,ModuleType}.cs`

### ADR-16: Specific business invariants worth flagging
- `Chatbot` protects two invariants: an owner can't be removed from the team (`ChatbotTeamMemberRemover`/`Chatbot.RemoveTeamMember` throw `ChatbotErrors.OwnerCannotBeRemoved` when the target index is the owner), and status transitions/archival are explicit, event-raising operations rather than free property sets.
- `Chatbot.Context` (HTML shown in the chat panel) is a distinct property from `Chatbot.Description` (the short listing blurb) — not duplicates.
- `Formation.domain.cs`'s module-completion path throws `FormationErrors.ModuleLockedByGating` when the previous module has gating enabled and hasn't been completed yet (mirrored by `FormationModuleCompleter`).
- `ConversationExchangeAppender`: when `ConversationId` is null, a new `Conversation` is started for the given chatbot — this command is the single write-side entry point for both "first message in a new chat" and "reply within an existing conversation".
- `LearnerProgress` is embedded in the `Formation` aggregate for the current scope (the frontend models a single demo learner); a real multi-user system should promote it to its own `LearnerFormationProgress` aggregate keyed by `userId`+`formationKey`, to avoid serializing unrelated learners' writes against the same lock. It's immutable — `WithModuleCompleted` returns a new instance rather than mutating.
- `FormationModule`: which fields are relevant depends on `Type` (video uses `ResourceUrl`+`DurationMin`, document/exercise use `ResourceUrl`, exam uses `Quiz`+`PassingScore`). It's immutable — `Formation.ToggleModuleGating` replaces the list entry with a `with`-copy rather than mutating a field on it.
- `AggregateRoot` (non-generic) is split from `AggregateRoot<TId>` so infrastructure code (`IEventBus`) can accept a heterogeneous batch of aggregates (`IEnumerable<AggregateRoot>`) without being tied to a single identity type.

**Where it applies:** `Chatbots/Domain/Chatbot.cs`, `Chatbots/Application/TeamMemberRemover/ChatbotTeamMemberRemover.cs`, `Formations/Domain/Formation.domain.cs`, `Formations/Application/ModuleCompleter/FormationModuleCompleter.cs`, `Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Formations/Domain/LearnerProgress.cs`, `Formations/Domain/FormationModule.cs`, `Shared/Domain/AggregateRoot.cs`
