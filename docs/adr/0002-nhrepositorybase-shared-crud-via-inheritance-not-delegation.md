# ADR-2: `NhRepositoryBase` — shared CRUD via inheritance, not delegation

- **Status:** Accepted
- **Area:** Persistence

`FindById`, `Save`, `Update` and `Remove` (CRUD) and `Search`, `Find` and `Count` (reads by specification) are `public` on the
base, not `protected`, precisely so a concrete `I...Repository` can declare any of them with that exact name and signature
and have it satisfied purely by inheriting from `NhRepositoryBase<TEntity, TId>` — no per-repo delegator method needed.
Each module's repository interface declares only the operations that module uses, spelled out in the interface itself
instead of inheriting a generic one, and the use cases build the specification (`BaseSpecification<T>`, in
`Shared/Domain/Specification`) that says what to read.

The three reads differ in what absence means. `Search` returns a list, empty when nothing matches. `Find` returns the
first match in the specification's order and fails (`InvalidOperationException`) when there is none, so a use case that
treats absence as a normal case, or must raise its own `DomainError` (`NotEnrolled`, `InvalidCredentials`...), uses
`Search` and takes the first element. `Count` counts the matches and ignores paging. `NhSpecificationConverter` turns the
criteria, the ordering and the paging of a specification into the NHibernate `IQueryable`. A `GroupBy` is the exception:
NHibernate cannot translate "first of each group", so it is applied in memory after the ordered query, which is fine for
a handful of rows and not for a large table.

`Update` is a pure alias to `Save` (NHibernate's `SaveOrUpdateAsync` already handles both insert and update
transparently); it exists as a separate name purely so call sites can express intent (creating vs. updating an existing
row). A repo whose access needs extra shaping still wraps the base: a singleton with no caller-supplied id
(`NhAnalyticsRepository.Find(CancellationToken)` calls `FindById(Analytics.SingletonKey, ...)`) or a lookup that needs more
than the row (`NhConversationHistoryRepository.FindById` hides the base method). Operations that cannot be expressed as a
read stay in the concrete repository, as raw SQL where atomicity requires it (`NhRefreshTokensRepository.Rotate` and
`RevokeAllOf`, see [ADR-20](0020-users-and-authentication-asp-net-core-identity-core.md)). `Sessions` is a `protected` accessor to the session factory for those.

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/{NhRepositoryBase,NhSpecificationConverter}.cs`,
`Shared/Domain/Specification/*` and every `I...Repository`/`Nh...Repository` pair.
