# ADR-17: NHibernate mapping conventions the schema depends on: a Guid dialect, enums by convention, `Lazy(false)` and explicit `text` columns

- **Status:** Accepted
- **Area:** Persistence

Four conventions keep the NHibernate mappings and the Postgres schema in agreement. None of them is caught by `dotnet build`:
a violation only shows when the session factory is built or a query runs.

- **Guid columns need a custom dialect.** NHibernate's stock `PostgreSQLDialect` has no type name registered for
  `DbType.Guid`, so as soon as NHibernate has to name the column type of any `GuidValueObjectUserType<T>`-backed column
  (schema export or validation) it throws `ArgumentException: Dialect does not support DbType.Guid`.
  `NakamaPostgreSqlDialect : PostgreSQLDialect` registers `DbType.Guid → "uuid"` in its constructor and is the dialect
  `NhSessionFactoryBuilder` uses.
- **Enum properties map by convention, not by explicit `.Type()`.**
  `Property(x => x.Status, m => m.Type(NHibernateUtil.Enum(typeof(ChatbotStatus))))` compiles but fails
  `Configuration.BuildSessionFactory()` with `MappingException: Could not determine type for: ChatbotStatus`: the `IType`
  built by `NHibernateUtil.Enum(Type)` does not survive the round trip through the mapping-by-code model the way a
  name-based type lookup does. A plain C# enum property maps to an ordinal `int` column with no `.Type()` override at all.
- **Every mapped entity uses `Lazy(false)`.** NHibernate requires every public method and property setter of a
  lazily-proxyable class to be `virtual` (`InvalidProxyTypeException` otherwise), not just the identity property. Nothing
  in this codebase relies on lazy-loading a single entity (there are no NHibernate associations, per
  [ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md)), so each `ClassMapping<T>` constructor
  disables proxy generation instead of putting `virtual` on dozens of domain members
  ([ADR-6](0006-protected-parameterless-constructors-exist-only.md)).
- **String columns default to `varchar(255)`**, including every `JsonUserType<T>`-backed column (`JsonUserType.SqlTypes`
  returns a bare `DbType.String`) and any genuinely long text (`ChatbotDetail.Context`, `Description`, ...).
  `Property(..., m => m.Length(int.MaxValue))` does not change the column type on this dialect; the working form is
  `m.Column(c => c.SqlType("text"))`, or `m.Column(c => { c.Name("..."); c.SqlType("text"); })` when the column also
  needs a custom name. The migration declares the same columns as `text` ([ADR-19](0019-fluentmigrator-runs-the-migrations-single-hand-written-baseline.md)).

**Where it applies:** `Shared/Infrastructure/Persistence/NhSessionFactoryBuilder.cs`,
`Shared/Infrastructure/Persistence/Nh/NakamaPostgreSqlDialect.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/*.cs`
