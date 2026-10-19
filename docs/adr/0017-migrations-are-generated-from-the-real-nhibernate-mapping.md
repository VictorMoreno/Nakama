# ADR-17: Migrations are generated from the real NHibernate mapping metadata, not hand-written

- **Status:** Superseded by [ADR-19](0019-fluentmigrator-replaces-dbup-as-the-migration-runner-every.md) (the migration mechanism); the mapping lessons it records still apply
- **Area:** Persistence — schema

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
  property ([ADR-6](0006-protected-parameterless-constructors-exist-only.md) only made `Id` virtual). Since nothing in this codebase relies on NHibernate lazy-loading a single
  entity (no NHibernate associations exist at all, per [ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md) - cross-aggregate references are raw ids), the correct fix
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
