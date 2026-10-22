# ADR-27: one unit of work per request, committed by an endpoint filter

- **Status:** Accepted
- **Area:** Persistence — transactions

Until now every repository method opened its own NHibernate session and `Save`/`Update`/`Remove` committed their own
transaction, so a command was several independent transactions: the aggregate, then one per read-model handler. A failure
halfway left the projections out of date. A request now has **one** `IUnitOfWork` (scoped, `Shared.Domain`) and every
repository works on its session. `NhUnitOfWork` opens the session lazily and begins the transaction on the first
`Save`/`Update`/`Remove`, never before: a request that only reads — including the minutes a `/ask` stream lasts — holds no
transaction. NHibernate defers the SQL of writes until the flush, so the commit is where everything reaches the database.
Domain events are still published synchronously right after `Save`, so the read-model handlers join the same unit: the
aggregate and its projections commit together or not at all. The separate read and write session factories are gone — both
pointed at the same database and could not see each other's pending changes; there is one `ISessionFactory`.

**Who commits.** An endpoint filter (`UnitOfWorkEndpointFilter`, on the root `api` group) runs the handler and, if it did
not throw and did not return a status of 400 or above, calls `SaveChanges`. A filter rather than a middleware on purpose:
a minimal API returns an `IResult` that is executed *after* the filters, so a failing commit is still a proper error
response, whereas a middleware only regains control once the response has been written and would report a success that was
not. Use cases and commands know nothing about the unit of work (the architecture tests forbid the `Application` layers
from depending on the persistence namespace). On an exception nothing is committed and disposing the DI scope rolls back.

**Two primitives, two exceptions.**
1. `SaveChanges` is idempotent, so a flow may commit earlier than the filter. `/ask` does: it writes its stream directly and
   `done` would otherwise go out before the commit, so a failing commit could not be reported. It commits after the last
   chunk and before `done`, and because it swallows the exception to emit `event: error` it must also call `Rollback`,
   otherwise the filter would see a normal return and commit what was pending.
2. `INhUnitOfWorkFactory.Create()` gives an independent unit. `RefreshTokens.RevokeAllOf` uses it: reusing a rotated token
   revokes every session of the person and then throws `InvalidRefreshToken`, and that revocation must survive the
   rollback of the request. The independent unit must not touch rows the request unit already wrote, or the two would wait
   for each other; in that flow `Rotate` has not modified anything.

**Releasing the connection.** NHibernate keeps a session's connection until the session closes unless a transaction ends,
and it rejects `after_statement` release. Reads therefore go through `INhUnitOfWork.Read`, which disconnects the session
after each read made outside a transaction and reconnects it on the next use, so a long stream does not pin a pooled
connection. Both the Identity user store and `NhRepositoryBase` use it.

**Consequences.** Entities stay attached for the whole request, so a change made to a loaded entity is persisted at commit
even without `Save`; `JsonUserType` compares the serialized JSON, so its dirty checking is correct. Constraint violations
now surface at commit and not at `Save`. A session is not thread-safe and must not be shared by parallel tasks; nothing in
the backend does that today. Files are still written and deleted outside the transaction (a document is deleted before the
commit), the same window as before, accepted on purpose. Code that writes outside an HTTP request — such as the tests that
create users with `UserManager` — must call `SaveChanges` itself.

**Where it applies:** `Shared/Domain/IUnitOfWork.cs`, `Shared/Infrastructure/Persistence/NhUnitOfWork.cs`,
`Shared/Infrastructure/Persistence/Nh/NhRepositoryBase.cs`, `api/Configuration/UnitOfWorkEndpointFilter.cs`,
`api/Configuration/EndpointExtensions.cs`, `NhRefreshTokensRepository`, `NhUserRepository`, `ChatbotEndpoints.AskChatbot`.
