# ADR-23: `UserActivity` records every login as its own row

- **Status:** Accepted
- **Area:** Users

A successful login produces one `UserActivityEntry` row. `User.MarkActive` records `UserActivityRecorded`;
`UserCredentialsVerifier.Verifier` marks the user active, saves it and publishes its events, and
`UserActivityEntryCreatedOnUserActivityRecorded` (`UserActivity/Application/Handlers/`) saves the row.

Publishing is what makes the row exist, and it is easy to omit without noticing: an aggregate that records an event and is
saved but not published loses the event silently — no exception, no log line — and the login response never depended on
it, so a 200 proves nothing. That is why `UserCredentialsVerifierShould` asserts that `UserActivityRecorded` is published,
and why the first wiring was checked by querying Postgres directly after two real logins (two rows, each with the right
`UserId` and timestamp).

`UserActivity` is a pure read model in the same shape as `Analytics` or `ChatbotSummaries`: no `AggregateRoot` and no
invariants of its own, just a `UserActivityEntry : IEntity` created with `UserActivityEntry.Create(...)`. `Type` is a plain
`string`, always `"Login"`, instead of an enum because the event only fires from a successful login; logout and token
refresh are not recorded as activity. `NhUserActivityRepository` derives from `NhRepositoryBase<UserActivityEntry, Guid>`
and implements `IUserActivityRepository` (`Save` only), so [ADR-22](0022-repositories-are-scrutor-scanned-by-base-class.md)'s Scrutor scan registers it with no manual
DI. Nothing reads the table yet (see `TODO.md`).

**Where it applies:** `UserActivity/**`, `Users/Domain/{User.domain,Events/UserActivityRecorded}.cs`,
`Users/Application/CredentialsVerifier/UserCredentialsVerifier.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/UserActivityEntryMapping.cs`,
`Shared/Infrastructure/Persistence/{NhMappings,Migrations/InitialSchemaMigration}.cs`.
