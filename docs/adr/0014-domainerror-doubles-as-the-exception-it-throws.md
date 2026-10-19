# ADR-14: `DomainError` doubles as the exception it throws

- **Status:** Accepted
- **Area:** Domain model notes

There is no separate wrapper exception — a concrete error (e.g. `ChatbotErrors.OwnerCannotBeRemoved`) is thrown directly
(`throw ChatbotErrors.OwnerCannotBeRemoved;`). Concrete errors live in each module's `Domain/Errors` folder with a
stable, project-prefixed code (`nakama.<module>...`).

**Where it applies:** `Shared/Domain/DomainError.cs` and every module's `Domain/Errors/*`
