# ADR-1: The session factory is a lazy, shared singleton

- **Status:** Accepted (amended by ADR-27: the separate read and write factories were merged into one)
- **Area:** Persistence

`NhSessionFactory` (the implementation of `ISessionFactory`) wraps an NHibernate `ISessionFactory`. Registered as a
singleton, built lazily on first use rather than at DI-container-build time — building one is expensive, and lazy build lets
the app start and compile/run without a reachable Postgres instance. `NhSessionFactoryBuilder` holds the actual build logic
(Postgres dialect, connection string from `ConnectionStrings:<name>`); database-per-tenant resolution is explicitly out of
scope for the current pass, left as a placeholder for later. It used to be two factories, `IWriteSessionFactory` and
`IReadSessionFactory`, over the same database; they could not see each other's pending changes, so ADR-27 replaced them with
one factory whose sessions are owned by the request's unit of work.

**Where it applies:**
`Shared/Infrastructure/Persistence/{ISessionFactory,NhSessionFactory,NhSessionFactoryBuilder}.cs`
