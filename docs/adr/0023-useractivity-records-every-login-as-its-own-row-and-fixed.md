# ADR-23: `UserActivity` records every login as its own row — and fixed a real bug where the event was raised but never published

- **Status:** Accepted
- **Area:** Users

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
`IUserActivityRepository`, so [ADR-22](0022-repositories-are-scrutor-scanned-by-base-class.md)'s Scrutor scan picks it up automatically — the first bounded context to actually
exercise that payoff.

**Where it applies:** `UserActivity/**` (new), `Shared/Infrastructure/Persistence/Nh/Mappings/UserActivityEntryMapping.cs`,
`Shared/Infrastructure/Persistence/{NhMappings,Migrations/InitialSchemaMigration}.cs`,
`Authentication/Application/Authenticator/LogIn.cs`.
