# Architecture Decision Records — Nakama backend

Each file in this folder records **one** decision: why it was taken and what it implies, written as the current state of
the system rather than as a history of edits. Code outside `apps/backend/api` carries no doc comments by convention, so
the non-obvious rationale lives here. [`agents.md`](../../apps/backend/src/Nakama/agents.md) documents *how* to write
code in the backend; the ADRs document *why* certain decisions were made.

## Conventions

- One decision per file, named `NNNN-short-title.md`, with the status and the area under the title.
- A decision that changes is rewritten in its own file; if a new decision replaces it, the old one gets the status
  `Superseded by ADR-N` and links to it. Decisions are not patched by adding follow-up entries.
- The numbers are the ones the decisions already had (`ADR-12b` and `ADR-13b` keep theirs). `ADR-24` does not exist: it
  described a migration that was removed.
- To add one, take the next free number and copy the header of any existing file.

## Index

| ADR | Decision | Area | Status |
|---|---|---|---|
| [ADR-1](0001-session-factories-are-lazy-shared-singletons.md) | Session factories are lazy, shared singletons | Persistence | Accepted |
| [ADR-2](0002-nhrepositorybase-shared-crud-via-inheritance-not-delegation.md) | `NhRepositoryBase` — shared CRUD via inheritance, not delegation | Persistence | Accepted |
| [ADR-3](0003-generic-value-object-persistence-via-reflection.md) | Generic value-object persistence via reflection | Persistence | Accepted |
| [ADR-4](0004-complex-nested-values-persist-as-a-single-json-column.md) | Complex nested values persist as a single JSON column | Persistence | Accepted |
| [ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md) | Cross-aggregate/cross-module references are raw identifiers, never the other side's Domain type | Persistence | Accepted |
| [ADR-6](0006-protected-parameterless-constructors-exist-only.md) | Protected parameterless constructors exist only for NHibernate proxy hydration | Persistence | Accepted |
| [ADR-7](0007-domain-events-never-reference-mediatr.md) | Domain events never reference MediatR | Domain events | Accepted |
| [ADR-8](0008-event-dispatch-is-synchronous-no-outbox.md) | Event dispatch is synchronous, no outbox | Domain events | Accepted |
| [ADR-9](0009-denormalized-read-model-entities-pattern.md) | Denormalized read-model entities pattern | Domain events | Accepted |
| [ADR-10](0010-cross-module-event-subscriptions-are-intentional-projection.md) | Cross-module event subscriptions are intentional projection coupling | Domain events | Accepted |
| [ADR-11](0011-analytics-defensive-dual-path-handler.md) | Analytics defensive dual-path handler | Domain events | Accepted |
| [ADR-12](0012-iquerybus-for-live-cross-module-reads-denormalized-entities.md) | `IQueryBus` for live cross-module reads, denormalized entities for per-event reads | CQRS / cross-module reads | Accepted |
| [ADR-12b](0012b-commands-and-queries-never-reference-mediatr-icommandbus.md) | Commands and queries never reference MediatR — `ICommandBus`/`IQueryBus` resolve handlers directly, no envelope | CQRS / cross-module reads | Accepted |
| [ADR-13](0013-deterministic-infrastructure-seams-instead-of-static-calls.md) | Deterministic infrastructure seams instead of static calls | Domain model notes | Accepted |
| [ADR-13b](0013b-ids-for-newly-created-aggregates-sub-entities-are-caller.md) | Ids for newly-created aggregates/sub-entities are caller-supplied, not server-generated | Domain model notes | Accepted |
| [ADR-14](0014-domainerror-doubles-as-the-exception-it-throws.md) | `DomainError` doubles as the exception it throws | Domain model notes | Accepted |
| [ADR-15](0015-several-domain-enums-intentionally-mirror-a-frontend.md) | Several Domain enums intentionally mirror a frontend TypeScript union | Domain model notes | Accepted |
| [ADR-16](0016-specific-business-invariants-worth-flagging.md) | Specific business invariants worth flagging | Domain model notes | Accepted |
| [ADR-17](0017-nhibernate-mapping-conventions-the-schema-depends-on.md) | NHibernate mapping conventions the schema depends on: a Guid dialect, enums by convention, `Lazy(false)` and explicit `text` columns | Persistence | Accepted |
| [ADR-18](0018-handler-is-a-thin-transformation-layer-the-actual-use-case.md) | `Handler` is a thin transformation layer; the actual use case is a separate, Scrutor-scanned class — Commands and Queries alike | CQRS / cross-module reads | Accepted |
| [ADR-19](0019-fluentmigrator-runs-the-migrations-single-hand-written-baseline.md) | FluentMigrator runs the migrations and the schema is a single hand-written baseline | Persistence — schema | Accepted |
| [ADR-20](0020-users-and-authentication-asp-net-core-identity-core.md) | Users and authentication — ASP.NET Core Identity Core over NHibernate, a short-lived JWT plus a rotating refresh token | Users | Accepted |
| [ADR-21](0021-entity-response-conversion-is-explicit-operator-by-default.md) | entity → Response conversion is `explicit operator` by default; `ToResponse` (extension method) when it can't be | Application layer | Accepted |
| [ADR-22](0022-repositories-are-scrutor-scanned-by-base-class.md) | Repositories are Scrutor-scanned by base class, and registered in `AddInfrastructure`, not `AddApplication` | Persistence | Accepted |
| [ADR-23](0023-useractivity-records-every-login-as-its-own-row.md) | `UserActivity` records every login as its own row | Users | Accepted |
| [ADR-25](0025-seed-data-is-a-manual-script-docs-db-demo-data-sql.md) | seed data is a manual script (`docs/db/demo-data.sql`), not a migration | Persistence — schema | Accepted |
