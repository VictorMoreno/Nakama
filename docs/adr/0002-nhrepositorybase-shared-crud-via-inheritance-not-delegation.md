# ADR-2: `NhRepositoryBase` — shared CRUD via inheritance, not delegation

- **Status:** Accepted
- **Area:** Persistence

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
