# ADR-21: entity → Response conversion is `explicit operator` by default; `ToResponse` (extension method) when it can't be

- **Status:** Accepted
- **Area:** Application layer

Every `<X>Response` record converts from its entity with
`public static explicit operator <X>Response(<Entity> entity) => new(...)`, called as `(Response)entity`. That is the
default, and it stays the default whenever the entity alone is enough (`UserResponse`, `ChatbotMemberResponse`, ...).

When the response needs something besides the entity, an operator cannot take extra arguments, so there are two shapes:

- A static factory on the Response record, `X.From(entity, extra)`: `ChatbotSummaryResponse.From(entity, iconsRoute)`,
  `ChatbotDetailResponse.From(...)` and `FormationDetailResponse.From(...)` (the icon route prefix comes from the storage
  options), and `AuthenticationResponse.From(user, token, refreshToken)`.
- An extension method `ToResponse`, defined in `Application`, when the extra values come from another read:
  `analytics.ToResponse(activeUsersCount, totalUsersCount)` (`AnalyticsExtensions`), because the active-user figures are
  queried live next to the `Analytics` singleton ([ADR-20](0020-users-and-authentication-asp-net-core-identity-core.md)).

Neither is an instance method on the entity: the entity lives in `Domain`, the response lives in `Application`, and an
instance method would make `Domain` reference an `Application`-layer DTO, inverting the dependency direction every other
bounded context respects.

**Where it applies:** `Analytics/Application/AnalyticsResponse.cs`, `ChatbotSummaries/Application/ChatbotSummaryResponse.cs`,
`ChatbotDetail/Application/ChatbotDetailResponse.cs`, `FormationDetail/Application/FormationDetailResponse.cs`,
`Authentication/Application/AuthenticationResponse.cs`
