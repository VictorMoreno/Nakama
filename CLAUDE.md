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

## Known drift: `apps/frontend/nakama.web/agents.md`

That guide describes a **target/reference architecture** (`modules/<feature>/domain+infrastructure`, dependency injection via `useInjection`/`Services`, CSS Modules SCSS per component) carried over from another project's template. It does **not** match how `nakama.web` is actually built today: the current app uses a flatter `src/features/<feature>/` + `src/context/` (plain React Context, no DI container) structure, and a single shared `src/styles/design-system.css` instead of per-component CSS Modules.

- Follow the **existing codebase conventions** (features/context/single stylesheet) for changes to existing code — don't silently migrate files toward the agents.md architecture as a side effect of an unrelated task.
- Only apply the agents.md conventions (modules/domain/infrastructure split, DI, CSS Modules) if the user explicitly asks to move toward that architecture, or when it's genuinely ambiguous how to structure something brand new and no existing precedent in the codebase answers it.
- If this drifts back into sync (someone migrates the app to match agents.md), remove this note.
