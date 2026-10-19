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

`Conversation` references `Chatbot` only by `ChatbotId` value, never by object reference or NHibernate association —
a bounded context must not reference another context's Domain (agents.md §1), and `Conversation` is deliberately its
own aggregate (see ADR-9 below for why). Mapping-wise, `ConversationMapping` declares it as a plain scalar column,
never an NHibernate association, for the same reason.

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

A read-model handler may subscribe to a *different* module's `Domain/Events` — for example
`FormationDetailCreatedOnFormationCreated` listens to `Formations`' `FormationCreated`. That is the intentional
projection coupling this pattern relies on: the read model never touches the write side's Domain entities or another
read model's repository, it only consumes the events. The `ChatbotLabelRef` lookup that once resolved a linked
chatbot's label for `FormationDetail` was removed together with the formation's linked chatbot, since nothing consumed it; neither the column nor the
table exists in the baseline schema (ADR-19).

**Where it applies:** `FormationDetail/Application/Handlers/FormationDetailCreatedOnFormationCreated.cs`,
`ChatbotSummaries/Application/Handlers/*`

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
foreign-key-shaped columns that reference a *different* aggregate's identity (`conversations.bot_key`)
— that keeps its `_key` suffix, since renaming them to `id` would make a foreign
reference look like a local primary key. Since this changed the physical schema and there was no production data to
preserve, the fix was a full local Postgres volume recreation (`docker compose down -v && docker compose up -d`) rather
than a second migration transforming the old columns in place — acceptable because `MigrationRunner.Run` only ever
executes in Development (ADR-17).

**The schema is a single baseline until the first production release.** While nothing is deployed, every schema change
is edited straight into `InitialSchemaMigration` instead of stacking new migrations: the refresh-tokens table that
briefly lived in its own migration and the removal of the formation's linked chatbot (the
`linked_chatbot_key`/`linked_chatbot_label` columns and the `read_formation_detail_chatbot_label_ref` table) were folded
back in, so the file always describes the whole schema as one `CREATE`, with no history to replay. The folded result was
checked against the three migrations it replaced by dumping `information_schema.columns` and `pg_indexes` from a
freshly migrated database before and after (identical) and by running NHibernate's `SchemaValidator` against it (no
mapping/schema mismatch).

Before freezing it the baseline was also hardened. Every column is `NOT NULL` except the ones whose C# property is
nullable (`iconurl` on the chatbot and formation tables and their read models, `users.passwordhash`,
`users.lastactiveat`, `refresh_tokens.revoked_at` and `replaced_by_id`); the JSON columns are `NOT NULL` too, since the
domain never leaves a collection or a settings object null. Two indexes were added where the code actually queries:
a unique `users.normalizedemail` (login and `UserByEmailSpecification` look users up by it, and two accounts must not
share an email) and `formation_enrollments (user_id, formation_id)` unique, which replaces the previous
`(formation_id, user_id)` one because its `user_id` prefix also serves `FormationEnrollmentByUserSpecification`. No
foreign keys on purpose: cross-aggregate references are raw ids (ADR-5). The hardened schema was verified with the whole
suite (including the E2E tests) and by loading `docs/db/demo-data.sql` into it with no constraint violation.

The cost is that any existing local database must be recreated
(`docker compose down -v && docker compose up -d`): its `VersionInfo` already records version 1 as applied, so
FluentMigrator would not re-run the rewritten file. From the first production deployment on, `InitialSchemaMigration`
is frozen and every change is a new numbered migration.

**Where it applies:** `Shared/Infrastructure/Persistence/{MigrationRunner,Migrations/InitialSchemaMigration}.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`, `Nakama.csproj`, `Directory.Packages.props`.

## Users

### ADR-20: Users and authentication — ASP.NET Core Identity Core over NHibernate, a short-lived JWT plus a rotating refresh token

**Decision.** `Users` and `Authentication` are two bounded contexts. `Users` owns the `User` aggregate and checks credentials
with ASP.NET Core Identity Core backed by NHibernate. `Authentication` owns the session: it issues a JWT access token and a
rotating refresh token, and the API authorizes every request from that token. OAuth/SSO is out of scope here:
`docs/architecture/stack-tecnologico.md` plans an own identity server now and Jaltest Auth later behind an OIDC/SAML port,
and leaves two choices open (Duende IdentityServer vs. OpenIddict, and validating Jaltest's contract) that this ADR does
not take.

**Identity Core works with NHibernate — verified, not assumed.** The `net10.0` dependency group of
`Microsoft.Extensions.Identity.Core` has zero EF Core: `UserManager<TUser>`, `IUserStore<TUser>` and `PasswordHasher<TUser>`
are storage-agnostic, and the EF coupling lives in the separate `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package,
which is not referenced (the shape of Microsoft's "Custom storage providers" guide). `NhUserRepository`
(`Users/Infrastructure`) is that store: it implements only `IUserPasswordStore<User>` and `IUserEmailStore<User>` — no roles,
claims, lockout or two-factor, because nothing asks for them — as thin translations onto `User`'s own setters. The
registration is `AddIdentityCore<User>().AddUserStore<NhUserRepository>()`, deliberately not `AddIdentity`, which would also
wire a default cookie scheme and `SignInManager`.

**`NhUserRepository` talks to NHibernate directly, never through `IUsersRepository`.** The domain repository
(`NhUsersRepository`) wraps `UserManager<User>` to verify passwords, and `UserManager<User>` needs an `IUserStore<User>` to be
built. If the store depended on `IUsersRepository` the container would have a cycle
(`IUserStore<User>` → `IUsersRepository` → `UserManager<User>` → `IUserStore<User>`) and refuse to build the service tree.
So the store takes `IWriteSessionFactory` and does its own minimal session work for find-by-id, save and
find-by-normalized-email. That small duplication of what `NhUsersRepository` also does is the accepted cost, not an
oversight.

**`User` is a plain `AggregateRoot<UserId>`**, not `IdentityUser`: Identity's `TUser` has no base-class requirement and reaches
every property only through the store, so `User` stays as domain-pure as `Chatbot` or `Formation`. `Email` is a domain
value object (`Users/Domain/Email.cs`) wrapping the raw string and a computed `Normalized` value, and rejecting a malformed
address (`UserErrors.InvalidEmail`, 422). It is not an `IStringValueObject` (ADR-3): `Email` and `NormalizedEmail` persist as two
independent columns because Identity sets each one separately, so `Email` only exists in Domain/Application code. The
store's own setters stay plain strings because Identity computes those values with its own normalizer. Use cases and
commands carry a plain `string Email` (ADR-18) and the `Handler` builds the value object. Users are provisioned by an
administrator, never self-registered, and the MVP has no user-creation endpoint: accounts come from
`docs/db/demo-data.sql` (ADR-25). The users listing is the only user endpoint.

**Checking credentials.** `UserCredentialsVerifier` is modeled as a Query, the one deliberate exception to ADR-18's
Command/Query split: commands are void-only and login's purpose is a value (who is this credential), with `MarkActive`'s
timestamp write as an accepted side effect of the lookup; a `TResponse` variant of `ICommand` just for this caller would
extend shared CQRS infrastructure for one use. The user is found with `UserByEmailSpecification` (by
`NormalizedEmail`) and its password is verified by `IUsersRepository.VerifyPassword`, so the use case never references
`Microsoft.AspNetCore.Identity`: both operations describe the same aggregate's contract. A wrong email and a wrong password
raise the same `InvalidCredentials` error, and a successful login raises `UserActivityRecorded` (ADR-23).

**Access token.** `HmacSha256JwtTokenGenerator` signs an HS256 JWT with claims `sub` (user id), `jti`, `email`, `name` and `role`
(`admin` or `member`). `JwtOptions` (`Jwt:Issuer`, `Audience`, `SigningKey`, `AccessTokenMinutes`, `RefreshTokenDays`) is validated on
startup — the signing key must be at least 32 bytes and comes from user-secrets or the environment, never from source —
and the bearer validation checks issuer, audience, signature, algorithm and lifetime with a 30 second skew. The access
token lasts 15 minutes.

**Refresh token.** `RefreshToken` is the aggregate of its own module, `Authentication`, because a second aggregate gets its own
module; it reaches `Users` only through `IQueryBus`. The value is 32 random bytes (Base64Url); only its SHA-256 hash is stored
(`token_hash`, unique). It travels in the cookie `nakama_refresh`: `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/auth`.
`POST /api/auth/login`, `/refresh` and `/logout` are the only anonymous endpoints besides `/health`. Every refresh rotates
the token: `NhRefreshTokensRepository.Rotate` revokes the current one and stores its replacement in one transaction with
`update ... where revoked_at is null`, so two concurrent refreshes cannot both win. Presenting a token that was already
rotated (or losing the rotation race) is treated as theft: `RevokeAllOf` closes every session of that user and the answer is
the same generic `InvalidRefreshToken`. Rotate and revoke-all are raw SQL on purpose, so they are not specifications.
`POST /api/auth/logout` revokes the token and clears the cookie.

**Authorization.** The identity always comes from the token: `ICurrentUser` (`HttpContextCurrentUser`) reads `sub` and the
role claim, and no endpoint accepts a `userId` or an `isAdmin` from the client. The fallback policy requires an authenticated
user, so a new endpoint is protected unless it opts out with `AllowAnonymous`. The `Admin` policy requires the admin role,
and `ChatbotMember` is a resource policy: `ChatbotMemberAuthorizationHandler` lets administrators through and otherwise asks
`ChatbotMemberSearcher` (module `ChatbotMembers`, through `IQueryBus`) whether the user belongs to the chatbot named in the
route (`id` or `botKey`). It protects the chatbot detail and the question endpoint, and the chatbot list is filtered by the
same membership. Team roles (owner, editor, reader) exist in the domain but are not enforced yet.

**HTTP statuses.** `ApiExceptionHandler` maps a `DomainError` by its code suffix: `.not_found` 404, `.already_exists` 409,
`.invalid_credentials`, `.invalid_refresh_token` and `.refresh_token_reused` 401, and anything else 422. A failed policy is a
403 from the framework.

**The client keeps the access token in memory only** (`AuthSession`), never in web storage, so a script cannot read it and
the refresh cookie stays unreachable from JavaScript. On load `UserProvider` calls refresh to restore the session
(`isRestoring` holds the UI until it answers). `authFetch` attaches the bearer token and, on a 401 for a non-auth endpoint,
refreshes once and retries; if that fails it expires the session and the user returns to the login.

**Active-users figures.** `AnalyticsFinder.Finder` assembles the whole `AnalyticsResponse` itself instead of returning the
`Analytics` entity, the one deliberate exception to the Finder pattern (ADR-18; the `ToResponse` extension is ADR-21). The response merges the `Analytics`
singleton's incrementing counters with a live count of users active in the last 7 days
(`UserActiveSinceSpecification`) and the total (`UserListingSpecification`). The rolling window is not part of `Analytics`'s
running totals because an incrementing counter cannot represent users dropping out of "active in the last 7 days" as time
passes; only a live query can.

**Not covered.** Login has no rate limit or lockout, the provider API keys are stored in plain text, the demo
accounts share a fixed password and there is no HSTS/CSP/CORS policy (all in `TODO.md`).

**Where it applies:** `Users/**`, `Authentication/**`, `ChatbotMembers/**`,
`Shared/Infrastructure/Auth/JwtOptions.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/{UserMapping,RefreshTokenMapping}.cs`,
`Shared/Infrastructure/Persistence/Migrations/InitialSchemaMigration.cs`, `Analytics/Application/Finder/AnalyticsFinder.cs`,
`apps/backend/api/{Configuration/{AuthExtensions,AuthPolicies,ChatbotMemberAuthorizationHandler,ApiExceptionHandler,RefreshTokenCookie,HttpContextCurrentUser,InfrastructureExtensions},Endpoints/{AuthEndpoints,UsersEndpoints}}.cs`,
and on the frontend `apps/frontend/nakama.web/src/{shared/infrastructure/auth/**,modules/auth/**,views/pages/auth/LoginPage.tsx,views/shared/contexts/UserContext.tsx}`.

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

`User.MarkActive` raises `UserActivityRecorded`, but until this entry nothing subscribed to it, and — found
live, not by reading the code — `UserCredentialsVerifier.Verifier.Execute` called `user.MarkActive(...)` and
`repository.Save(user, ct)` but never `eventBus.Publish(user, ct)`, unlike the other use cases that mutate an aggregate, which do. The event was
being raised into the aggregate's own `DomainEvents` list and then silently dropped, forever — no exception, no log
line, nothing, because there was nothing downstream to fail. Two logins against a running instance produced zero rows
in the new table until `eventBus.Publish(user, ct)` was added to `Verifier.Execute`; the same two logins
afterward produced two rows, each with the right `UserId` and timestamp — verified by querying Postgres directly, not
by trusting a 200 response, since the endpoint's response never depended on the event actually being published.

New bounded context, `UserActivity`, is a pure read-model in the same shape as `Analytics`/`ChatbotSummaries` — no
`AggregateRoot`, no invariants of its own, just `UserActivityEntry.Create(...)` populated by
`UserActivityEntryCreatedOnUserActivityRecorded` (`UserActivity/Application/Handlers/`) reacting to
`UserActivityRecorded`. `Type` is a plain `string`, hardcoded `"Login"` by the handler rather than modeled as an enum
yet, because `UserActivityRecorded` only ever fires today from a successful login — there is no logout flow to
produce a second value (logout and token refresh are not recorded as activity). `NhUserActivityRepository` needed zero
manual DI registration: it derives from `NhRepositoryBase<UserActivityEntry, Guid>` and implements
`IUserActivityRepository`, so ADR-22's Scrutor scan picks it up automatically — the first bounded context to actually
exercise that payoff.

**Where it applies:** `UserActivity/**` (new), `Shared/Infrastructure/Persistence/Nh/Mappings/UserActivityEntryMapping.cs`,
`Shared/Infrastructure/Persistence/{NhMappings,Migrations/InitialSchemaMigration}.cs`,
`Authentication/Application/Authenticator/LogIn.cs`.

### ADR-25: seed data is a manual script (`docs/db/demo-data.sql`), not a migration

Seed rows are not schema, so they do not belong in a migration: a migration runs on every database it touches —
including the Testcontainers template —, would hard-code passwords into the assembly and cannot be skipped. The
migrations only create the schema, and the demo data — the two login accounts (`admin@nakama.com` and `user@nakama.com`)
plus 20 users, 15 chatbots, 10 formations, 72 conversations and the read models that go with them — lives in
`docs/db/demo-data.sql`, which a person runs by hand (`docker exec -i nakama-postgres psql -U nakama -d nakama < docs/db/demo-data.sql`).

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
