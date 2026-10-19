# ADR-21: entity → Response conversion is `explicit operator` by default; `ToResponse` (extension method) when it can't be

- **Status:** Accepted
- **Area:** Application layer

Every `<X>Response` record converts from its entity via `public static explicit operator <X>Response(<Entity> entity) => new(...)`,
called at the site as `(Response)entity` — this is the default and stays the default whenever the entity alone is
enough to build the response.

When it isn't — extra parameters are needed beyond the entity itself, as `AnalyticsResponse` now needs
`activeUsersCount`/`totalUsersCount` alongside the `Analytics` singleton ([ADR-20](0020-users-and-authentication-asp-net-core-identity-core.md)) — an operator can't take extra
arguments, so the conversion is a method named `ToResponse`, not `From`. It is **not** an instance method on the
entity itself: the entity lives in `Domain`, the response lives in `Application`, and an instance method would mean
`Domain` referencing an `Application`-layer DTO, inverting the dependency direction every other bounded context in
this codebase respects. Instead it's a C# extension method — defined in `Application` (`static class
AnalyticsExtensions { public static AnalyticsResponse ToResponse(this Analytics entity, ...) }`) — which keeps the
entity type itself untouched while still reading as `analytics.ToResponse(activeUsersCount, totalUsersCount)` at the
call site, the same shape as the operator cast it replaces.

**Where it applies:** `Analytics/Application/AnalyticsResponse.cs`, `Analytics/Application/Finder/AnalyticsFinder.cs`.
