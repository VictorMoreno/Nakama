# ADR-1: Session factories are lazy, shared singletons

- **Status:** Accepted
- **Area:** Persistence

`IWriteSessionFactory`/`IReadSessionFactory` (both shaped by `ISessionFactory`) wrap an NHibernate `ISessionFactory`.
Registered as singletons, built lazily on first use rather than at DI-container-build time — building one is expensive,
and lazy build lets the app start and compile/run without a reachable Postgres instance. `ISessionFactory` is the shared
shape so `NhRepositoryBase<TEntity, TId>` can accept either side without depending on the write/read-specific DI token.
`NhSessionFactoryBuilder` holds the actual build logic (Postgres dialect, connection string from
`ConnectionStrings:<name>`); database-per-tenant resolution is explicitly out of scope for the current pass, left as a
placeholder for later.

**Where it applies:**
`Shared/Infrastructure/Persistence/{IWriteSessionFactory,IReadSessionFactory,ISessionFactory,NhWriteSessionFactory,NhReadSessionFactory,NhSessionFactoryBuilder}.cs`
