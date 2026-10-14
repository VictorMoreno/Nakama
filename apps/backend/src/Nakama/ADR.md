# Architecture Decision Records — Nakama backend

Non-obvious rationale that used to live as XML `<summary>` comments across `Domain`/`Application`/`Infrastructure`. Code
outside `apps/backend/api` carries no doc comments by convention — it must be self-descriptive through naming/structure.
Anything that isn't obvious from the code itself lives here instead. `agents.md` (in this same folder) documents *how*
to write code here; this file documents *why* certain decisions were made.

## Persistence

### ADR-1: Session factories are lazy, shared singletons

`IWriteSessionFactory`/`IReadSessionFactory` (both shaped by `ISessionFactory`) wrap an NHibernate `ISessionFactory`.
Registered as singletons, built lazily on first use rather than at DI-container-build time — building one is expensive,
and lazy build lets the app start and compile/run without a reachable Postgres instance. `ISessionFactory` is the shared
shape so `NhRepositoryBase<TEntity, TId>` can accept either side without depending on the write/read-specific DI token.
`NhSessionFactoryBuilder` holds the actual build logic (Postgres dialect, connection string from
`ConnectionStrings:<name>`); database-per-tenant resolution is explicitly out of scope for the current pass, left as a
placeholder for later.

**Where it applies:**
`Shared/Infrastructure/Persistence/{IWriteSessionFactory,IReadSessionFactory,ISessionFactory,NhWriteSessionFactory,NhReadSessionFactory,NhSessionFactoryBuilder}.cs`

### ADR-2: `NhRepositoryBase` — shared CRUD via inheritance, not delegation

`FindById`, `Save`, and `Update` are `public` on the base (not `protected`) precisely so a concrete `I...Repository`
whose identity lookup is a plain primary-key fetch can declare `FindById(TId, CancellationToken)` with that exact name
and signature and have it satisfied purely by inheriting from `NhRepositoryBase<TEntity, TId>` — no per-repo delegator
method needed. `Update` is a pure alias to `Save` (NHibernate's `SaveOrUpdateAsync` already handles both insert and
update transparently); it exists as a separate name purely so call sites can express intent (creating vs. updating an
existing row). A repo whose lookup needs extra shaping — e.g. a singleton with no caller-supplied id
(`NhAnalyticsRepository.Find`) — still wraps `FindById` in its own one-liner, since that can't be satisfied by
inheritance alone.

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/NhRepositoryBase.cs` and every `I...Repository`/
`Nh...Repository` pair.

### ADR-3: Generic value-object persistence via reflection

Identity value objects are positional record structs implementing either `IStringValueObject` (`Value: string`) or
`IGuidValueObject` (`Value: Guid`). `ValueObjectUserType<T>`/`GuidValueObjectUserType<T>` (siblings) map any such type
generically onto a varchar/native-uuid column respectively, round-tripping through the type's single-argument
constructor via `Activator.CreateInstance` — this avoids writing a bespoke NHibernate `IUserType` per identity type.

**Where it applies:** `Shared/Domain/{IStringValueObject,IGuidValueObject}.cs`,
`Shared/Infrastructure/Persistence/Nh/UserTypes/{ValueObjectUserType,GuidValueObjectUserType}.cs`

### ADR-4: Complex nested values persist as a single JSON column

`JsonUserType<T>` persists any complex .NET value (a list, a record, a nested value-object graph — e.g. the quiz
embedded in a formation module, or a chatbot's documents/team lists) as one JSON text column, per agents.md §6. This
avoids modelling every nested collection as its own NHibernate component/bag mapping. Used for `Chatbot.Settings` and
the private `_documents`/`_team` backing fields (`ChatbotMapping`).

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/UserTypes/JsonUserType.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/ChatbotMapping.cs`

### ADR-5: Cross-aggregate/cross-module references are raw identifiers, never the other side's Domain type

`Formation.LinkedChatbotKey` is a plain `Guid`, never `Chatbots.Domain.ChatbotId` — a bounded context must not reference
another context's Domain (agents.md §1). Same rule for `Conversation`, which references `Chatbot` only by `ChatbotId`
value, never by object reference or NHibernate association — it's deliberately its own aggregate (see ADR-9 below for
why). Mapping-wise, `FormationMapping`/`ConversationMapping` declare these as plain scalar columns, never NHibernate
associations, for the same reason.

**Where it applies:** `Formations/Domain/Formation.cs`, `Chatbots/Domain/Conversation.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/{FormationMapping,ConversationMapping}.cs`

### ADR-6: Protected parameterless constructors exist only for NHibernate proxy hydration

`Chatbot()`, `Conversation()`, `Formation()` (and similar) have a `protected`/empty constructor that is never meant to
be called by hand — it exists solely so NHibernate can generate lazy-load proxies and hydrate instances via reflection.
Same reasoning behind `AggregateRoot<TId>.Id` having a non-public setter: it keeps mutation confined to the aggregate's
own behavior methods, and NHibernate still reaches it via reflection when hydrating. No entity member is `virtual`:
that is only needed for lazy-loading proxies, and every mapping disables them with `Lazy(false)` (see ADR-17).

**Where it applies:** every aggregate's parameterless constructor; `Shared/Domain/AggregateRoot.cs`

## Domain events

### ADR-7: Domain events never reference MediatR

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

### ADR-8: Event dispatch is synchronous, no outbox

`IEventBus.Publish` takes a single aggregate (every use-case Handler in this codebase loads, mutates, and saves exactly
one aggregate per transaction — see ADR-5, cross-aggregate work happens via events/read-models, never a shared
transaction) and publishes every pending domain event it raised, then clears them. It's invoked explicitly at the end of
each use-case Handler, after the write-side transaction has committed — NHibernate has no `SaveChangesInterceptor`-style
hook, and this scope has no outbox, so dispatch is synchronous within the same request. A crash between commit and
publish would lose the event; acceptable for the current scope, worth revisiting if delivery guarantees matter later.

**Where it applies:** `Shared/Infrastructure/Events/{IEventBus,EventBus}.cs`

### ADR-9: Denormalized read-model entities pattern

A read entity (`ChatbotSummary`, `ChatbotDetail`, `FormationSummary`, `FormationDetail`, `Analytics`)
implements `IEntity` like any domain entity but is **not** an `AggregateRoot`: it never raises its own events, it only
consumes events raised by a real aggregate, and it's fed exclusively through its own module's `Application/Handlers`.
One read model exists per route, not per tab/section — e.g. `ChatbotDetail` covers all 4 tabs (General, Documents, Team,
Analytics) of `/chatbots/:key` in one entity, including a cross-aggregate derivation
(`Apply(ConversationExchangeAppended)` computes the Analytics tab's "most repeated questions" from `Conversation`
events). Where no real signal exists yet for a field (e.g. `ChatbotDetail.Gaps`, "was this answer good"), it's
deliberately left empty rather than faking a heuristic. `Analytics` is the degenerate case: not an aggregate, no identity of its own, a singleton row (`SingletonKey` is
always `"global"`) computed entirely as a derived projection across `Chatbot`/`Conversation`/`Formation` events — see
ADR-11 for its specific defensive-handler shape.

**Where it applies:** `ChatbotSummaries/Domain/ChatbotSummary.cs`, `ChatbotDetail/Domain/ChatbotDetail.cs`,
`FormationsList/Domain/FormationSummary.cs`, `FormationDetail/Domain/FormationDetail.cs`,
`Analytics/Domain/Analytics.cs`

### ADR-10: Cross-module event subscriptions are intentional projection coupling

`ChatbotLabelRef` is `FormationDetail`'s own local denormalized copy of a chatbot's label, keyed by the chatbot's id. It
is **not** exposed through `IFormationDetailRepository`/Application — it exists purely so `FormationDetail`'s own
projection handler can resolve `Chatbot.Label` for the "Basado en la documentación de X" caption without
`FormationDetail`'s Infrastructure ever touching `Chatbots`' Domain entities or its read model's repository directly.
`ChatbotLabelRefCreatedOnChatbotCreated` listens to `ChatbotCreated` — a totally different module's `Domain/Events` —
which is the intentional projection coupling this pattern relies on. There is no `ChatbotLabelChanged` event (a label
never changes after creation), so `ChatbotLabelRef` has no other `Apply` method. `FormationDetail.LinkedChatbotLabel` is
resolved at creation time (`FormationDetailCreatedOnFormationCreated`) via this local lookup, kept current by the
handler above, so the caption never needs a live cross-context join.

**Where it applies:** `FormationDetail/Domain/ChatbotLabelRef.cs`,
`FormationDetail/Domain/IChatbotLabelRefRepository.cs`,
`FormationDetail/Application/Handlers/ChatbotLabelRefCreatedOnChatbotCreated.cs`,
`FormationDetail/Application/Handlers/FormationDetailCreatedOnFormationCreated.cs`,
`FormationDetail/Domain/FormationDetail.cs`

### ADR-11: Analytics defensive dual-path handler

`Analytics` has no dedicated creation event of its own — it's fed by whichever of several event types (`ChatbotCreated`,
`ConversationExchangeAppended`, ...) arrives first. Its handlers are therefore defensive: on the first feeding event to
arrive, create the row via a `CreateFromX` factory and `Save`; if the row already exists (some other feeding event got
there first), apply the event onto it and `Update` instead.

**Where it applies:**
`Analytics/Application/Handlers/{AnalyticsCreatedOnChatbotCreated,AnalyticsCreatedOnConversationExchangeAppended}.cs`

## CQRS / cross-module reads

### ADR-12: `IQueryBus` for live cross-module reads, denormalized entities for per-event reads

`IQueryBus.Ask<TResponse>` lets a class ask a query belonging to another bounded context and get back its `Response`,
without depending on that context's `Domain`/repository directly. Use `IQueryBus` when a class needs a value that
"belongs" to another module and an eventual-consistency copy isn't wanted; reach for a denormalized read-model entity
(ADR-9/ADR-10) instead when the read happens on every event and a live cross-module call per event would be wasteful.

**Where it applies:** `Shared/Domain/{IQueryBus,IQuery}.cs`, `Shared/Infrastructure/Queries/QueryBus.cs`

### ADR-12b: Commands and queries never reference MediatR — `ICommandBus`/
`IQueryBus` resolve handlers directly, no envelope

Same intent as ADR-7 (domain events never reference MediatR), extended to the request/response side: no `Command`/
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

## Domain model notes

### ADR-13: Deterministic infrastructure seams instead of static calls

Factory methods take `ISystemClock` instead of calling `DateTime.UtcNow` directly, so domain behavior (timestamps) stays
deterministic and testable via injection.

**Where it applies:** `Shared/Domain/ISystemClock.cs`, every aggregate factory method that stamps a time (e.g.
`Chatbot.Create`)

### ADR-13b: Ids for newly-created aggregates/sub-entities are caller-supplied, not server-generated

`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`, and `ConversationExchangeAppender` (when starting a new
conversation) used to generate their new entity's id server-side via `IUuidGenerator` and return it in a `Response`,
which was the only reason those 4 commands returned anything — every other command in the codebase already returns an
empty `Response`. Since a client-generated GUID v4 has negligible collision probability, the id is now supplied by the
caller instead: the client already knows the id before sending the command, so the command can stay a pure side effect
with an empty `Response`, and `IUuidGenerator`/`GuidUuidGenerator` were deleted as dead code (they had no other
callers). This does introduce a risk that server-generated ids never had: a client could resend or accidentally reuse an
id, and `Save`'s NHibernate `SaveOrUpdate` semantics would silently overwrite the existing row instead of failing. The 3
pure-create handlers (`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`) guard against this explicitly with a
`FindById`-then-throw-`AlreadyExists` check before creating. `ConversationExchangeAppender` is the one exception:
finding an existing row for the supplied id is its *intended* "continue this conversation" path, not a collision, so it
has no such guard — it's a legitimate find-or-start.

**Where it applies:** `Chatbots/Application/Creator/ChatbotCreator.cs`,
`Formations/Application/Creator/FormationCreator.cs`, `Formations/Application/ModuleAdder/FormationModuleAdder.cs`,
`Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Chatbots/Domain/Errors/ChatbotErrors.cs`,
`Formations/Domain/Errors/FormationErrors.cs`

### ADR-14: `DomainError` doubles as the exception it throws

There is no separate wrapper exception — a concrete error (e.g. `ChatbotErrors.OwnerCannotBeRemoved`) is thrown directly
(`throw ChatbotErrors.OwnerCannotBeRemoved;`). Concrete errors live in each module's `Domain/Errors` folder with a
stable, project-prefixed code (`nakama.<module>...`).

**Where it applies:** `Shared/Domain/DomainError.cs` and every module's `Domain/Errors/*`

### ADR-15: Several Domain enums intentionally mirror a frontend TypeScript union

`ModuleType`, `FormationStatus`, `ChatbotStatus`, `LlmProvider`, `DocStatus` each mirror an equivalent frontend union
type, kept in sync by convention (not shared code) since these are two separate stacks. `TeamRole` is a partial mirror:
the frontend distinguishes grammatical gender for the owner label ("Propietario"/"Propietaria") for display purposes
only — the domain models a single `Owner` role, rendering the gendered label is a presentation concern. `TeamMember`'s
`isOwnerRole`-equivalent check is the domain invariant carrier behind that same frontend concept.

**Where it applies:** `Chatbots/Domain/{ChatbotStatus,LlmProvider,DocStatus,TeamRole,TeamMember}.cs`,
`Formations/Domain/{FormationStatus,ModuleType}.cs`

### ADR-16: Specific business invariants worth flagging

- `Chatbot` protects two invariants: an owner can't be removed from the team (`ChatbotTeamMemberRemover`/
  `Chatbot.RemoveTeamMember` throw `ChatbotErrors.OwnerCannotBeRemoved` when the target index is the owner), and status
  transitions/archival are explicit, event-raising operations rather than free property sets.
- `Chatbot.Context` (HTML shown in the chat panel) is a distinct property from `Chatbot.Description` (the short listing
  blurb) — not duplicates.
- `Formation.domain.cs`'s module-completion path throws `FormationErrors.ModuleLockedByGating` when the previous module
  has gating enabled and hasn't been completed yet (mirrored by `FormationModuleCompleter`).
- `ConversationExchangeAppender`: when `ConversationId` is null, a new `Conversation` is started for the given chatbot —
  this command is the single write-side entry point for both "first message in a new chat" and "reply within an existing
  conversation".
- `LearnerProgress` is embedded in the `Formation` aggregate for the current scope (the frontend models a single demo
  learner); a real multi-user system should promote it to its own `LearnerFormationProgress` aggregate keyed by
  `userId`+`formationKey`, to avoid serializing unrelated learners' writes against the same lock. It's immutable —
  `WithModuleCompleted` returns a new instance rather than mutating.
- `FormationModule`: which fields are relevant depends on `Type` (video uses `ResourceUrl`+`DurationMin`,
  document/exercise use `ResourceUrl`, exam uses `Quiz`+`PassingScore`). It's immutable — `Formation.ToggleModuleGating`
  replaces the list entry with a `with`-copy rather than mutating a field on it.
- `AggregateRoot` (non-generic) is split from `AggregateRoot<TId>` so infrastructure code (`IEventBus`) can accept a
  heterogeneous batch of aggregates (`IEnumerable<AggregateRoot>`) without being tied to a single identity type.

**Where it applies:** `Chatbots/Domain/Chatbot.cs`,
`Chatbots/Application/TeamMemberRemover/ChatbotTeamMemberRemover.cs`, `Formations/Domain/Formation.domain.cs`,
`Formations/Application/ModuleCompleter/FormationModuleCompleter.cs`,
`Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Formations/Domain/LearnerProgress.cs`,
`Formations/Domain/FormationModule.cs`, `Shared/Domain/AggregateRoot.cs`

## Persistence — schema

### ADR-17: Migrations are generated from the real NHibernate mapping metadata, not hand-written

`0001_InitialSchema.sql` (and every migration after it) is produced by running NHibernate's own `SchemaExport` against
the exact same `Configuration` object `NhSessionFactoryBuilder` builds for the running app (same dialect, same
mappings) — this guarantees the DDL matches what the mappings actually expect instead of a manually-maintained parallel
description that can drift. `DbUp` (`dbup-postgresql`) applies the resulting `.sql` files in order against Postgres,
tracked in its own `SchemaVersions` journal table; scripts are embedded resources (`Nakama.csproj`'s `EmbeddedResource`
glob on `Shared/Infrastructure/Persistence/Migrations/*.sql`), applied via `MigrationRunner.Run(connectionString)`.
Wired to run automatically on API startup **only in Development**
(`WebApplicationConfigurationExtensions.Configure()`) — a dev convenience so a fresh clone + `docker-compose up` +
`dotnet run` produces a working schema with no manual step; not a production deployment strategy, and not invoked at all
outside Development.

Generating the DDL this way surfaced three real, previously-latent mapping bugs that a hand-written migration would
never have caught, since none of them broke a normal `dotnet build`, only actual schema/session-factory generation:

- **Guid columns need a custom dialect.** NHibernate's stock `PostgreSQLDialect` has no DDL type-name registered for
  `DbType.Guid` — `SchemaExport` throws `ArgumentException: Dialect does not support DbType.Guid` the moment it tries to
  generate a CREATE TABLE for any `GuidValueObjectUserType<T>`-backed column. Fixed by
  `NakamaPostgreSqlDialect : PostgreSQLDialect`, registering `DbType.Guid → "uuid"` in its constructor; used everywhere
  `PostgreSQLDialect` used to be (`NhSessionFactoryBuilder`).
- **Enum properties map by convention, not by explicit `.Type()`.**
  `Property(x => x.Status, m => m.Type(NHibernateUtil.Enum(typeof(ChatbotStatus))))` compiles fine but fails
  `Configuration.BuildSessionFactory()`'s validation with
  `MappingException: Could not determine type for: ChatbotStatus` — the `IType` instance built by
  `NHibernateUtil.Enum(Type)` doesn't survive being round-tripped through the mapping-by-code model the way a name-based
  type lookup does. NHibernate maps a plain C# enum property to an ordinal `int` column automatically without any
  `.Type()` override at all — removing the explicit call is the fix, not a different way of calling it.
- **Every mapped entity needs `Lazy(false)`.** NHibernate validates that literally every public method and property
  setter on a lazily-proxyable mapped class is `virtual` (`InvalidProxyTypeException` otherwise) — not just the identity
  property (ADR-6 only made `Id` virtual). Since nothing in this codebase relies on NHibernate lazy-loading a single
  entity (no NHibernate associations exist at all, per ADR-5 - cross-aggregate references are raw ids), the correct fix
  is disabling proxy generation per mapping (`Lazy(false)` in every `ClassMapping<T>` constructor), not retrofitting
  `virtual` onto dozens of domain methods for a capability nothing uses.
- **String columns default to `varchar(255)`** unless told otherwise, including every `JsonUserType<T>`-backed column
  (`JsonUserType.SqlTypes` returns a bare `DbType.String`) and any genuinely long text field (`Chatbot.Context`/
  `Description`, etc.). `Property(..., m => m.Length(int.MaxValue))` does **not** change the generated column type on
  this dialect — the working fix is forcing the column's raw SQL type directly: `m.Column(c => c.SqlType("text"))` (or,
  when the column also needs a custom name, `m.Column(c => { c.Name("..."); c.SqlType("text"); })`).

**Where it applies:** `Shared/Infrastructure/Persistence/{MigrationRunner,NhSessionFactoryBuilder}.cs`,
`Shared/Infrastructure/Persistence/Nh/NakamaPostgreSqlDialect.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`,
`Shared/Infrastructure/Persistence/Migrations/*.sql`,
`apps/backend/api/Configuration/WebApplicationConfigurationExtensions.cs`

### ADR-18:
`Handler` is a thin transformation layer; the actual use case is a separate, Scrutor-scanned class — Commands and Queries alike

Every Command's `Handler.Handle` does nothing but convert every primitive `Command` field into its domain-typed value —
identity value objects (`ChatbotId.From(cmd.Id)`), enums (`(ChatbotStatus)cmd.Status`), and domain records
(`new ChatbotSettings(cmd.Socratic, cmd.ExplicaMas, (LlmProvider)cmd.Provider)`) alike, no exceptions for "just" an
enum — and delegate to a second nested class whose `Execute` signature is therefore *fully domain-typed*, never touching
a raw `Command` field or doing a cast itself. That second class does the real work
(find-or-throw/find-or-create/find-or-start, mutate, `Save`, `Publish`). Every Query's `Handler.Handle` mirrors this
from the other side: the second nested class does the real work (fetch-or-throw `NotFound`, or `FindAll`), and `Handler`
holds only the output transformation (the `(Response)entity` cast, or the `.Select(e => (Response)e).ToList()`
projection for a list). That second class implements `IService` (`Shared/Application/IService.cs`) — an empty marker
interface whose only purpose is to be a Scrutor scan target (`InfrastructureExtensions.cs`'s `AddApplication()`,
`classes.AssignableTo<IService>().AsSelf()`). It's registered `AsSelf()`, not `AsImplementedInterfaces()`, deliberately:
every `Handler` injects the *concrete* use-case class (`Creator`, `Adder`, `Remover`, `Updater`, `Archiver`, `Toggler`,
`Completer`, `Appender`, `Finder`, `Searcher`...), never `IService` itself — dozens of unrelated classes share that one
marker, so resolving by the interface would be ambiguous.

**Naming the nested class**: it is named after what it does, matching the outer static class's own suffix —
`ChatbotCreator.Creator`, `ChatbotDocumentAdder.Adder`, `ChatbotDocumentRemover.Remover`,
`ChatbotStatusUpdater.Updater`, `ConversationExchangeAppender.Appender`, `FormationModuleGatingToggler.Toggler`,
`FormationModuleCompleter.Completer`, `ChatbotDetailFinder.Finder`,
`FormationDetailFinder.Finder`, `AnalyticsFinder.Finder`, `ChatbotSummarySearcher.Searcher`,
`FormationSummarySearcher.Searcher`. It is *not* always called `Updater` — that name is reserved for cases that are
genuinely an update of an existing aggregate's field (`ChatbotStatusUpdater`, `ChatbotSettingsUpdater`,
`ChatbotIconUpdater`, `FormationStatusUpdater`, `FormationIconUpdater`).

**When to use a `<Aggregate>FinderById` domain service** (`Chatbots/Domain/Services/ChatbotFinderById.cs`,
`Formations/Domain/Services/FormationFinderById.cs`) inside the use-case class, vs. not:

- **Find existing-or-throw `NotFound`, then mutate** (most commands: `Archiver`, `Updater` variants, `Adder`/`Remover`
  variants acting on an existing aggregate): use the finder — it centralizes
  `repository.FindById(id, ct) ?? throw <Aggregate>Errors.NotFound(id)` in one place per aggregate instead of repeating
  it in every use case.
- **Check the aggregate does NOT already exist, then create it** (`ChatbotCreator`, `FormationCreator`): the inverse
  check — a `NotFound`-throwing finder doesn't fit, keep the `FindById(...) is not null` check inline.
- **Find-or-implicitly-create (upsert), never throws** (`ConversationExchangeAppender`): doesn't fit a throwing finder
  either, keep the `FindById(...) ?? Start(...)` inline.
- **A sub-resource-level check inside an aggregate already found via the finder** (`FormationModuleAdder`'s "module must
  not already exist" check, after finding the *Formation* via `FormationFinderById`): stays inline, same reasoning as
  the Creators — it's a single caller, not worth its own service class.

**Queries don't get a `Domain/Services` finder of their own.** `ChatbotFinderById`/`FormationFinderById` exist because
the *same* "find this aggregate or throw `NotFound`" logic is genuinely repeated across many different Commands for the
same aggregate. Every read-model Query (`ChatbotDetailFinder`, `FormationDetailFinder`,
`AnalyticsFinder`, and the two Searchers) has exactly one consumer — its own `Handler` — so the `Finder`/`Searcher`
nested class already *is* the use case; there's no second caller to justify pulling the fetch logic out into a shared
`Domain/Services` class the way the write side does.

**Where it applies:** `Shared/Application/IService.cs`, every `<UseCase>.Handler`/`<UseCase>.<Action>` pair under
`**/Application/*/`, `Chatbots/Domain/Services/ChatbotFinderById.cs`,
`Formations/Domain/Services/FormationFinderById.cs`, the `IService` Scrutor scan in
`apps/backend/api/Configuration/InfrastructureExtensions.cs`.

### ADR-19: FluentMigrator replaces DbUp as the migration runner; every table's primary key column is `id`, not `key`

`MigrationRunner.Run(connectionString)` now builds a FluentMigrator `IMigrationRunner`
(`AddFluentMigratorCore().ConfigureRunner(rb => rb.AddPostgres().WithGlobalConnectionString(...).ScanIn(assembly).For.Migrations())`)
and calls `MigrateUp()`, instead of DbUp's `DeployChanges.To.PostgresqlDatabase(...)`. FluentMigrator tracks applied
migrations in its own `VersionInfo` table (three of its own bootstrap migrations create/extend it on first run) — same
role DbUp's `SchemaVersions` journal used to have.

`[Migration(1)] InitialSchemaMigration` defines the schema with FluentMigrator's own fluent API
(`Create.Table("chatbots").WithColumn("id").AsGuid().PrimaryKey()...`), not by executing a `.sql` file — the
embedded-script approach (`Execute.EmbeddedScript(...)`) was the first cut but was deliberately replaced with
hand-written fluent code per explicit request, trading away part of ADR-17's guarantee: the schema is no longer
regenerated from the live NHibernate mapping metadata on every change, so a future mapping change (new property, renamed
column, different nullability) does **not** automatically keep the migration in sync — whoever changes a
`ClassMapping<T>` now also has to hand-update `InitialSchemaMigration.Up()` to match. The column types and names in this
first migration were transcribed 1:1 from a `SchemaExport` run against the mappings as they stood at the time (verified
against the live Postgres schema via `information_schema.columns` before/after), so there is no drift *yet* - only going
forward.

**Every non-snake_case column name in `InitialSchemaMigration` is written lowercase on purpose** (`label`, not `Label`),
even though the NHibernate mapping is `Property(x => x.Label)`. `FluentMigrator.Runner.Postgres` quotes every identifier
it emits in `CREATE TABLE`, preserving exact case (`"Label"`); NHibernate's own generated DML is unquoted, and Postgres
case-folds an unquoted identifier to lowercase before matching it against the catalog. A quoted `"Label"` and a folded
`label` are two different identifiers to Postgres — the first version of this migration used `.WithColumn("Label")` and
every read endpoint failed at runtime with
`42703 column chatbotsum0_.label does not exist (Hint: Perhaps you meant "Label")`, caught by hitting the real endpoints
against a freshly-migrated database, not by `dotnet build`. Lowercasing every column name side-steps the mismatch
entirely, matching what the old SchemaExport-generated unquoted SQL produced.

Every `ClassMapping<T>`'s `Id(x => x.Id, m => m.Column("..."))` that used to name its primary-key column `key` (or, for
two read-model tables whose id happens to equal their owning chatbot's id, `bot_key`) now names it `id` — consistent,
predictable naming across all ten mapped tables, no exceptions for read-model tables. This does **not** apply to genuine
foreign-key-shaped columns that reference a *different* aggregate's identity (`conversations.bot_key`,
`formations.linked_chatbot_key`) — those keep their `_key` suffix, since renaming them to `id` would make a foreign
reference look like a local primary key. Since this changed the physical schema and there was no production data to
preserve, the fix was a full local Postgres volume recreation (`docker compose down -v && docker compose up -d`) rather
than a second migration transforming the old columns in place — acceptable because `MigrationRunner.Run` only ever
executes in Development (ADR-17).

**Where it applies:** `Shared/Infrastructure/Persistence/{MigrationRunner,Migrations/InitialSchemaMigration}.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`, `Nakama.csproj`, `Directory.Packages.props`.

## Users

### ADR-20:
`Users` is a real bounded context, backed by NHibernate-flavored ASP.NET Core Identity Core — deliberately without OAuth/SSO/cookie sessions yet

`docs/architecture/stack-tecnologico.md` documents the long-term auth plan (an own identity server now, replaced by
Jaltest Auth later, behind an OIDC/SAML port) but leaves two decisions explicitly open (Duende IdentityServer vs.
OpenIddict; validating Jaltest's OIDC/SAML contract). This ADR is **not** that pass. It only replaces the fully-fake
login (`LoginPage.tsx` used to `e.preventDefault(); navigate("/chat")` with zero backend call, accepting any
credentials) with a real one: a `Users` bounded context, real password hashing/verification, and a real `LastActiveAt`
timestamp — needed to stop the Analytics "Usuarios activos esta semana" card from being hardcoded (`34`,
`de 48 con acceso`). No cookie/JWT/session is issued; a page reload still loses the "logged in" state. No per-endpoint
authorization exists anywhere in this API either way (today's `isAdmin` is a pure frontend toggle) — this pass doesn't
add any, since real authorization is meaningless without a real session to check.

**ASP.NET Core Identity works with NHibernate, verified, not assumed.** Checked the published nuspec for
`Microsoft.Extensions.Identity.Core` (the `net10.0` dependency group): only
`Microsoft.AspNetCore.Cryptography.KeyDerivation`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Options` — zero
EF Core. `UserManager<TUser>`, `IUserStore<TUser>` (+ `IUserPasswordStore<TUser>`, `IUserEmailStore<TUser>`), and
`PasswordHasher<TUser>` all live in this package and are 100% storage-agnostic; EF coupling only exists in the separate,
never-referenced `Microsoft.AspNetCore.Identity.EntityFrameworkCore` adapter — same officially-supported shape as
Microsoft's "Custom storage providers for ASP.NET Core Identity" doc. `NhUserStore`
(`Users/Infrastructure/NhUserStore.cs`) implements exactly the minimal trio (`IUserStore<User>`,
`IUserPasswordStore<User>`, `IUserEmailStore<User>` — no roles/claims/lockout/2FA, nothing here asks for that) as thin
translations onto `IUsersRepository`/`User`'s own domain setters. DI is
`services.AddIdentityCore<User>().AddUserStore<NhUserStore>()` — deliberately `AddIdentityCore`, not `AddIdentity`: the
latter also wires a default cookie scheme + `SignInManager`, exactly the session/OAuth-adjacent machinery this pass
skips.

`User` is a normal `AggregateRoot<UserId>` domain class (not `Microsoft.AspNetCore.Identity.IdentityUser`) — Identity's
`TUser` generic parameter has no base-class requirement; every property Identity touches is reached only through
`IUserStore<User>`'s methods, which this codebase's own `NhUserStore` implements, so `User` stays exactly as domain-pure
as `Chatbot`/`Formation`. Users are admin-provisioned (`POST /api/users`, matching RF6 "Panel de administración:
Gestionar usuarios"), not self-registered — `UserAuthenticator` is the only unauthenticated-facing surface
(`POST /api/auth/login`).

**`UserAuthenticator` is modeled as a Query, not a Command** — the one deliberate exception to ADR-18's Command/Query
split. `ICommand`/`ICommandBus` in this codebase are void-only (confirmed: none of the other 14 commands carry a
response), and login's purpose from the caller's side is fundamentally a value (is this credential real, who is it) with
`MarkActive`'s timestamp write as an accepted side effect of the lookup — inventing an `ICommand<TResponse>` variant
just for this one caller would mean extending shared CQRS infrastructure for a single use.

**`AnalyticsFinder.Finder` now assembles the full `AnalyticsResponse` itself**, not just the domain `Analytics` entity —
the one deliberate exception to the rest of this codebase's Finder pattern (every other Finder returns a domain
entity/collection; `Handler` does the `(Response)entity` cast). Reason: this response now merges two independent read
sources — the `Analytics` singleton's lifetime incrementing counters (`ActiveBotsCount`, `AnsweredThisWeek`), and a
live, rolling 7-day-window count over `Users` (`ActiveUsersCount`, via
`IUsersRepository.CountActiveSince(systemClock.UtcNow.AddDays(-7), ct)`) plus `TotalUsersCount`. The rolling-window
count is **not** folded into `Analytics`'s own incrementing-counter model, because an incrementing counter can never
represent "active in the last 7 days" correctly — users need to be able to drop *out* of the count as time passes, which
only a live query (not an event-sourced running total) can do.

**`NhRepositoryBase<TEntity, TId>` gained a `protected ISessionFactory Sessions => sessions;` accessor.** Every
repository before `NhUsersRepository` only ever needed `FindById`/`FindAllEntities`/`Save`/`Remove` from the base —
`NhUsersRepository` is the first that needs a genuinely custom query (`GetByEmail`, `CountAll`,
`CountActiveSince`) the base doesn't expose, and the base's session factory was a private primary-constructor capture
with no subclass access at all. The new accessor is the same `OpenSession()`-per-call shape every existing base method
already uses — any future repository needing a custom query reuses it instead of taking its own separate session-factory
dependency.

**`DomainErrorExceptionHandler` gained one more status-code rule**: `.invalid_credentials` → `401 Unauthorized`,
alongside the existing `.not_found` → 404 and `.already_exists` → 409 (everything else still falls through to 422).
Wrong credentials are neither a not-found nor a conflict, and 401 is the correct HTTP status for them — same extensible
`switch` the other two rules already live in.

**`Email` is a domain value object** (`Users/Domain/Email.cs`) wrapping the raw string plus a computed `Normalized`
property, used by the Users use cases (`UserCredentialsVerifier` today) so no caller — nor any future Users use case — has to
remember to call `.ToUpperInvariant()` itself; the constructor also rejects an obviously malformed value
(`UserErrors.InvalidEmail`, falls through to 422). It is **not** an `IStringValueObject`/`ValueObjectUserType<T>`
(ADR-3): that pattern round-trips an identity value object through a *single* DB column via
`Activator.CreateInstance`, but `Email`/`NormalizedEmail` persist as two independent columns because
`IUserEmailStore<User>`'s contract sets each one separately (Identity's own store methods call
`SetEmailAsync`/`SetNormalizedEmailAsync` independently) — `Email` only exists in Domain/Application code, never
persisted as a unit. Command/Query DTOs (`UserCredentialsVerifier.Query`) still carry a plain `string
Email`, per ADR-18 — the `Handler` is what constructs `new Email(cmd.Email)`, same as every other domain-typed
conversion in this codebase. `NhUserStore`'s own setters (`SetEmail`/`SetNormalizedEmail` on `User`) stay raw `string`
on purpose: Identity computes those values itself (via its own normalizer) when calling into the store, never through
this VO.

**`IUsersRepository.GetByEmail` takes the `Email` VO, not a normalized string** — an earlier version was named
`FindByNormalizedEmail(string normalizedEmail, ...)`, which leaked the lookup's implementation detail (that it
compares against a normalized index) into the interface contract, and still required every caller to remember to
normalize before calling it. There is exactly one lookup method on the interface, not a second raw-string one for
infrastructure's sake.

**User provisioning is out of the MVP.** The `UserCreator` use case, `POST /users` and everything that existed only for them
(`UserEmailAvailabilityChecker`, `IUsersRepository.CreateWithPassword`, the `UserCreated` event and the
`EmailAlreadyInUse`/`Invalid` errors) were removed; users come from `docs/db/demo-data.sql` (ADR-25). `GetByEmail` stays because
credential verification needs it.

**`IUsersRepository` also owns credential verification** (`VerifyPassword(User, string, ct)` on `NhUsersRepository`, wrapping
`UserManager<User>`) — `UserCredentialsVerifier` no longer references `Microsoft.AspNetCore.Identity` at all. This was an
explicit call, made after weighing the alternative of a separate `IUserAccountManager` port, on the grounds that both
concerns describe the same aggregate's contract.

**Consequence, found live, not assumed: `NhUserStore` can no longer depend on `IUsersRepository`.**
`NhUsersRepository` now depends on `UserManager<User>` for the credential methods; `UserManager<User>` needs an
`IUserStore<User>` to be constructed at all; `NhUserStore` used to satisfy its own `FindById`/`Save`/find-by-email
needs by calling into `IUsersRepository`. Wiring all three together is a cycle
(`IUserStore<User>(NhUserStore) → IUsersRepository(NhUsersRepository) → UserManager<User> → IUserStore<User>`), and
the .NET DI container refuses to build the service tree at all when it exists — confirmed by actually starting the
API with the naive wiring and reading its own error before touching anything further, not by reasoning about it on
paper. `NhUserStore` now takes `IWriteSessionFactory` directly and does its own minimal NHibernate session
work for `FindById`/`Save`/find-by-normalized-email, instead of delegating to the repository — a small, deliberate
duplication of what `NhUsersRepository` also does, and the accepted cost of the decision above, not an oversight.

**Where it applies:** `Users/**` (new),
`Shared/Infrastructure/Persistence/Nh/{NhRepositoryBase,Mappings/UserMapping}.cs`,
`Shared/Infrastructure/Persistence/Migrations/InitialSchemaMigration.cs`,
`Analytics/Application/{AnalyticsResponse,Finder/AnalyticsFinder}.cs`,
`apps/backend/api/{Configuration/{InfrastructureExtensions,DomainErrorExceptionHandler},Endpoints/UsersEndpoints}.cs`,
`Nakama.csproj`, `Directory.Packages.props`, and on the frontend
`apps/frontend/nakama.web/src/{views/pages/auth/LoginPage.tsx,modules/analytics/**,views/pages/analytics/{AnalyticsPage.tsx,analyticsData.ts}}`.

### ADR-21: entity → Response conversion is `explicit operator` by default; `ToResponse` (extension method) when it can't be

Every `<X>Response` record converts from its entity via `public static explicit operator <X>Response(<Entity> entity) => new(...)`,
called at the site as `(Response)entity` — this is the default and stays the default whenever the entity alone is
enough to build the response.

When it isn't — extra parameters are needed beyond the entity itself, as `AnalyticsResponse` now needs
`activeUsersCount`/`totalUsersCount` alongside the `Analytics` singleton (ADR-20) — an operator can't take extra
arguments, so the conversion is a method named `ToResponse`, not `From`. It is **not** an instance method on the
entity itself: the entity lives in `Domain`, the response lives in `Application`, and an instance method would mean
`Domain` referencing an `Application`-layer DTO, inverting the dependency direction every other bounded context in
this codebase respects. Instead it's a C# extension method — defined in `Application` (`static class
AnalyticsExtensions { public static AnalyticsResponse ToResponse(this Analytics entity, ...) }`) — which keeps the
entity type itself untouched while still reading as `analytics.ToResponse(activeUsersCount, totalUsersCount)` at the
call site, the same shape as the operator cast it replaces.

**Where it applies:** `Analytics/Application/AnalyticsResponse.cs`, `Analytics/Application/Finder/AnalyticsFinder.cs`.

### ADR-22: Repositories are Scrutor-scanned by base class, and registered in `AddInfrastructure`, not `AddApplication`

Every `I<X>Repository`/`Nh<X>Repository` pair used to be bound one line at a time
(`services.AddScoped<IChatbotsRepository, NhChatbotsRepository>();`, repeated for all eleven) inside
`AddApplication()` — wrong method (a repository binding is an Infrastructure concern, not Application), and it meant
every new bounded context needed a manual line added here on top of writing the repository itself.

Both are fixed together. The binding moved to `AddInfrastructure()`, and it's no longer manual: every
`Nh<X>Repository` already derives from `NhRepositoryBase<TEntity, TId>` (ADR-2) and implements exactly one custom
`I<X>Repository` interface, so `services.Scan(scan => scan.FromAssemblyOf<Chatbot>().AddClasses(classes =>
classes.AssignableTo(typeof(NhRepositoryBase<,>))).AsImplementedInterfaces().WithScopedLifetime())` finds all eleven
by their shared open-generic base and binds each to whatever single interface it implements — same
`FromAssemblyOf<Chatbot>()` anchor the existing Handler/`IService` scan in `AddApplication()` already uses, since
this is still one assembly. A future bounded context's repository needs zero lines added here: derive from
`NhRepositoryBase<,>`, implement its own interface, done. Verified live (not just a clean build, which reflection-based
DI wiring can pass even when broken) by exercising an endpoint from every bounded context — chatbots, formations,
analytics, chatbot detail, chat page, and login — against a running instance after the change.

**`AssemblyHelper.Domain`** (`Shared/Infrastructure/AssemblyHelper.cs`) replaced the three separate `typeof(Chatbot)`/
`FromAssemblyOf<Chatbot>()` anchors this file used to have (`AddMediatR`, and both `Scan()` calls) with one static
`Assembly` property, so the "which type do we anchor reflection scans on" choice lives in exactly one place. It's
still `typeof(Chatbot).Assembly` under the hood — Domain/Application/Infrastructure are namespaces inside one
physical assembly (`Nakama.csproj`), not separate ones, so `.Domain` here names *the anchor's layer*, not a distinct
assembly. The `Api` host project (genuinely a separate assembly) keeps its own `typeof(Program)` anchor in
`EndpointExtensions.cs`'s `IEndpoint` scan unchanged — `Nakama.csproj` can't reference `Program` (`Api` depends on
`Nakama`, never the reverse), so a single cross-project helper isn't possible here; each project anchors its own
scans. Scrutor's `FromAssembly(Assembly)` doesn't exist — the actual method is `FromAssemblies(params Assembly[])`,
found by letting the compiler error rather than assuming.

**Where it applies:** `apps/backend/api/Configuration/InfrastructureExtensions.cs`,
`Shared/Infrastructure/AssemblyHelper.cs`.

### ADR-23: `UserActivity` records every login as its own row — and fixed a real bug where the event was raised but never published

`User.MarkActive` (ADR-20) raises `UserActivityRecorded`, but until this entry nothing subscribed to it, and — found
live, not by reading the code — `UserAuthenticator.Authenticator.Execute` called `user.MarkActive(...)` and
`repository.Save(user, ct)` but never `eventBus.Publish(user, ct)`, unlike the other use cases that mutate an aggregate, which do. The event was
being raised into the aggregate's own `DomainEvents` list and then silently dropped, forever — no exception, no log
line, nothing, because there was nothing downstream to fail. Two logins against a running instance produced zero rows
in the new table until `eventBus.Publish(user, ct)` was added to `Authenticator.Execute`; the same two logins
afterward produced two rows, each with the right `UserId` and timestamp — verified by querying Postgres directly, not
by trusting a 200 response, since the endpoint's response never depended on the event actually being published.

New bounded context, `UserActivity`, is a pure read-model in the same shape as `Analytics`/`ChatbotSummaries` — no
`AggregateRoot`, no invariants of its own, just `UserActivityEntry.Create(...)` populated by
`UserActivityEntryCreatedOnUserActivityRecorded` (`UserActivity/Application/Handlers/`) reacting to
`UserActivityRecorded`. `Type` is a plain `string`, hardcoded `"Login"` by the handler rather than modeled as an enum
yet, because `UserActivityRecorded` only ever fires today from a successful login — there is no logout flow to
produce a second value (no session/cookie exists to log out of, per ADR-20). `NhUserActivityRepository` needed zero
manual DI registration: it derives from `NhRepositoryBase<UserActivityEntry, Guid>` and implements
`IUserActivityRepository`, so ADR-22's Scrutor scan picks it up automatically — the first bounded context to actually
exercise that payoff.

**Where it applies:** `UserActivity/**` (new), `Shared/Infrastructure/Persistence/Nh/Mappings/UserActivityEntryMapping.cs`,
`Shared/Infrastructure/Persistence/{NhMappings,Migrations/InitialSchemaMigration}.cs`,
`Users/Application/Authenticator/UserAuthenticator.cs`.

### ADR-24: `SeedUsersMigration` — two dev-only accounts, so a freshly-recreated database has someone to log in as

**Superseded by ADR-25:** the migration was removed and the accounts now live in `docs/db/demo-data.sql`. The reasoning below is kept as history.

`InitialSchemaMigration` only ever created tables, never rows — every `docker compose down -v && up -d` (routine
throughout this project's local workflow) left `users` empty, with no way in until someone first called
`POST /api/users` by hand. `[Migration(2)] SeedUsersMigration` inserts `admin@nakama.com` and `user@nakama.com`
(password `Admin123.` for both) directly into the `users` table via `Insert.IntoTable(...).Row(...)`. Neither name
implies a real elevated role — no authorization/roles exist yet (ADR-20), so "admin" here is only a naming
convention, exactly like the "admin-provisioned" wording in that same ADR entry.

The password hash is computed inside the migration with `new PasswordHasher<User>().HashPassword(user, password)` —
the identical class `UserManager<User>.CreateAsync` uses internally (ADR-20) — rather than a hand-written or
precomputed hash string, so it is guaranteed to verify correctly through the normal login path with zero risk of a
format mismatch. Building a real `User` via `User.Create(...)` (not a bare anonymous object) to pass into
`HashPassword` was deliberate too: the default `PasswordHasher<TUser>` implementation happens to ignore its `user`
argument entirely, but relying on that undocumented behavior (e.g. passing `null!`) is exactly the kind of thing that
silently breaks if Identity ever changes it — constructing a real entity costs nothing and removes the risk. Verified
live: recreated Postgres from scratch, confirmed `SeedUsersMigration migrated` ran right after `InitialSchemaMigration`,
and logged in as both seeded accounts through the real `POST /api/auth/login` endpoint.

**Where it applies:** `Shared/Infrastructure/Persistence/Migrations/SeedUsersMigration.cs`.

### ADR-25: seed data is a manual script (`docs/db/demo-data.sql`), not a migration

`SeedUsersMigration` (ADR-24) and `SeedDemoTestDataMigration` ran on every database the migrations touched, which
is the wrong place for rows that are not schema: they were applied to every fresh database including the
Testcontainers template, hard-coded passwords into the assembly, and could not be skipped. Both are gone. The
migrations now only create tables, and everything they used to insert — the two login accounts plus 20 users, 15
chatbots, 10 formations, 72 conversations and the read models that go with them — lives in `docs/db/demo-data.sql`, which
a person runs by hand (`docker exec -i nakama-postgres psql -U nakama -d nakama < docs/db/demo-data.sql`).

The script was not written by hand. The old generator was run once against a throwaway PostgreSQL and its output was
dumped with `pg_dump --data-only --column-inserts --on-conflict-do-nothing`, then the generator was deleted. That
keeps the exact JSON shapes and password hashes the application produces, and `ON CONFLICT DO NOTHING` makes the
script safe to run twice. The `pg_dump` preamble was trimmed to `client_encoding` and `standard_conforming_strings`
(`SET transaction_timeout` does not exist before PostgreSQL 17 and the `\restrict` meta-commands are psql-only).

Consequences: a fresh database has **no users**, so nobody can log in until the script is run; the dates inside it
are those of the day it was generated; `read_analytics` only exists once a chatbot is created or the script is loaded.
No test covers the script on purpose: it is demonstration data, nothing depends on it, and a schema change that
breaks it shows up as an error the moment someone runs it. If that turns out to be annoying, a single test that
loads it into a freshly migrated database (no row counts, no passwords) is the most that would be worth adding.

**Where it applies:** `docs/db/demo-data.sql`.
