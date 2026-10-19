# ADR-8: Event dispatch is synchronous, no outbox

- **Status:** Accepted
- **Area:** Domain events

`IEventBus.Publish` takes a single aggregate (every use-case Handler in this codebase loads, mutates, and saves exactly
one aggregate per transaction — see [ADR-5](0005-cross-aggregate-cross-module-references-are-raw-identifiers.md), cross-aggregate work happens via events/read-models, never a shared
transaction) and publishes every pending domain event it raised, then clears them. It's invoked explicitly at the end of
each use-case Handler, after the write-side transaction has committed — NHibernate has no `SaveChangesInterceptor`-style
hook, and this scope has no outbox, so dispatch is synchronous within the same request. A crash between commit and
publish would lose the event; acceptable for the current scope, worth revisiting if delivery guarantees matter later.

**Where it applies:** `Shared/Infrastructure/Events/{IEventBus,EventBus}.cs`
