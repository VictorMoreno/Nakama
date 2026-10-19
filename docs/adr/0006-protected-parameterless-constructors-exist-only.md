# ADR-6: Protected parameterless constructors exist only for NHibernate proxy hydration

- **Status:** Accepted
- **Area:** Persistence

`Chatbot()`, `Conversation()`, `Formation()` (and similar) have a `protected`/empty constructor that is never meant to
be called by hand — it exists solely so NHibernate can generate lazy-load proxies and hydrate instances via reflection.
Same reasoning behind `AggregateRoot<TId>.Id` having a non-public setter: it keeps mutation confined to the aggregate's
own behavior methods, and NHibernate still reaches it via reflection when hydrating. No entity member is `virtual`:
that is only needed for lazy-loading proxies, and every mapping disables them with `Lazy(false)` (see [ADR-17](0017-nhibernate-mapping-conventions-the-schema-depends-on.md)).

**Where it applies:** every aggregate's parameterless constructor; `Shared/Domain/AggregateRoot.cs`
