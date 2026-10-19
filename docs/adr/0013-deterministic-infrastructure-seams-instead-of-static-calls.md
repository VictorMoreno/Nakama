# ADR-13: Deterministic infrastructure seams instead of static calls

- **Status:** Accepted
- **Area:** Domain model notes

Factory methods take `ISystemClock` instead of calling `DateTime.UtcNow` directly, so domain behavior (timestamps) stays
deterministic and testable via injection.

**Where it applies:** `Shared/Domain/ISystemClock.cs`, every aggregate factory method that stamps a time (e.g.
`Chatbot.Create`)
