# Nakama monorepo — agents.md routing

This repo has one `agents.md` per module, each defining the implementation conventions for that stack. They are **not loaded by default** — consult the one(s) relevant to the files you are about to touch, not all of them on every task.

## Routing table

| Working in...                | Read this guide first          | Stack |
|-------------------------------|----------------------------------|-------|
| `apps/frontend/nakama.web/**` | `apps/frontend/nakama.web/agents.md` | React + TypeScript + Vite |
| `apps/backend/api/**`         | `apps/backend/api/agents.md`   | ASP.NET Core API layer |
| `apps/backend/src/**`         | `apps/backend/src/agents.md`   | .NET domain/application core (DDD + hexagonal) |
| `apps/tests/**`                | `apps/tests/agents.md`          | .NET tests (NUnit + FluentAssertions) |

Rules:

- Before creating or restructuring files in one of these paths, read that path's `agents.md` and follow its naming, layering, and checklist conventions.
- A task spanning multiple modules (e.g. a new API endpoint backed by a new domain use case) needs the guides for **every** module it touches — e.g. `apps/backend/api/agents.md` **and** `apps/backend/src/agents.md`.
- A task confined to one module only needs that module's guide.
- Small fixes that don't add new files/structure (typo, one-line bug fix, style tweak) don't need the guide re-read — use judgement.

## Known drift: `apps/backend/api/agents.md`

That guide describes controllers MVC + `ICommandBus`/`IQueryBus` custom, carried over from
another project's template. The actual code uses Minimal API via the `IEndpoint` + Scrutor
pattern, and MediatR (`ISender`/`IPublisher`) instead of the custom bus. Documented in full
(with rationale) at the top of `apps/backend/api/agents.md` itself — read that note before
touching `apps/backend/api`. Domain-layer conventions in `apps/backend/src/Nakama/agents.md`
are followed literally, no drift there.
