# ADR-20: Users and authentication — ASP.NET Core Identity Core over NHibernate, a short-lived JWT plus a rotating refresh token

- **Status:** Accepted
- **Area:** Users

**Decision.** `Users` and `Authentication` are two bounded contexts. `Users` owns the `User` aggregate and checks credentials
with ASP.NET Core Identity Core backed by NHibernate. `Authentication` owns the session: it issues a JWT access token and a
rotating refresh token, and the API authorizes every request from that token. OAuth/SSO is out of scope here:
`docs/architecture/stack-tecnologico.md` plans an own identity server now and Jaltest Auth later behind an OIDC/SAML port,
and leaves two choices open (Duende IdentityServer vs. OpenIddict, and validating Jaltest's contract) that this ADR does
not take.

**Identity Core works with NHibernate — verified, not assumed.** The `net10.0` dependency group of
`Microsoft.Extensions.Identity.Core` has zero EF Core: `UserManager<TUser>`, `IUserStore<TUser>` and `PasswordHasher<TUser>`
are storage-agnostic, and the EF coupling lives in the separate `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package,
which is not referenced (the shape of Microsoft's "Custom storage providers" guide). `NhUserRepository`
(`Users/Infrastructure`) is that store: it implements only `IUserPasswordStore<User>` and `IUserEmailStore<User>` — no roles,
claims, lockout or two-factor, because nothing asks for them — as thin translations onto `User`'s own setters. The
registration is `AddIdentityCore<User>().AddUserStore<NhUserRepository>()`, deliberately not `AddIdentity`, which would also
wire a default cookie scheme and `SignInManager`.

**`NhUserRepository` talks to NHibernate directly, never through `IUsersRepository`.** The domain repository
(`NhUsersRepository`) wraps `UserManager<User>` to verify passwords, and `UserManager<User>` needs an `IUserStore<User>` to be
built. If the store depended on `IUsersRepository` the container would have a cycle
(`IUserStore<User>` → `IUsersRepository` → `UserManager<User>` → `IUserStore<User>`) and refuse to build the service tree.
So the store takes `IWriteSessionFactory` and does its own minimal session work for find-by-id, save and
find-by-normalized-email. That small duplication of what `NhUsersRepository` also does is the accepted cost, not an
oversight.

**`User` is a plain `AggregateRoot<UserId>`**, not `IdentityUser`: Identity's `TUser` has no base-class requirement and reaches
every property only through the store, so `User` stays as domain-pure as `Chatbot` or `Formation`. `Email` is a domain
value object (`Users/Domain/Email.cs`) wrapping the raw string and a computed `Normalized` value, and rejecting a malformed
address (`UserErrors.InvalidEmail`, 422). It is not an `IStringValueObject` ([ADR-3](0003-generic-value-object-persistence-via-reflection.md)): `Email` and `NormalizedEmail` persist as two
independent columns because Identity sets each one separately, so `Email` only exists in Domain/Application code. The
store's own setters stay plain strings because Identity computes those values with its own normalizer. Use cases and
commands carry a plain `string Email` ([ADR-18](0018-handler-is-a-thin-transformation-layer-the-actual-use-case.md)) and the `Handler` builds the value object. Users are provisioned by an
administrator, never self-registered, and the MVP has no user-creation endpoint: accounts come from
`docs/db/demo-data.sql` ([ADR-25](0025-seed-data-is-a-manual-script-docs-db-demo-data-sql.md)). The users listing is the only user endpoint.

**Checking credentials.** `UserCredentialsVerifier` is modeled as a Query, the one deliberate exception to [ADR-18](0018-handler-is-a-thin-transformation-layer-the-actual-use-case.md)'s
Command/Query split: commands are void-only and login's purpose is a value (who is this credential), with `MarkActive`'s
timestamp write as an accepted side effect of the lookup; a `TResponse` variant of `ICommand` just for this caller would
extend shared CQRS infrastructure for one use. The user is found with `UserByEmailSpecification` (by
`NormalizedEmail`) and its password is verified by `IUsersRepository.VerifyPassword`, so the use case never references
`Microsoft.AspNetCore.Identity`: both operations describe the same aggregate's contract. A wrong email and a wrong password
raise the same `InvalidCredentials` error, and a successful login raises `UserActivityRecorded` ([ADR-23](0023-useractivity-records-every-login-as-its-own-row.md)).

**Access token.** `HmacSha256JwtTokenGenerator` signs an HS256 JWT with claims `sub` (user id), `jti`, `email`, `name` and `role`
(`admin` or `member`). `JwtOptions` (`Jwt:Issuer`, `Audience`, `SigningKey`, `AccessTokenMinutes`, `RefreshTokenDays`) is validated on
startup — the signing key must be at least 32 bytes and comes from user-secrets or the environment, never from source —
and the bearer validation checks issuer, audience, signature, algorithm and lifetime with a 30 second skew. The access
token lasts 15 minutes.

**Refresh token.** `RefreshToken` is the aggregate of its own module, `Authentication`, because a second aggregate gets its own
module; it reaches `Users` only through `IQueryBus`. The value is 32 random bytes (Base64Url); only its SHA-256 hash is stored
(`token_hash`, unique). It travels in the cookie `nakama_refresh`: `HttpOnly`, `Secure`, `SameSite=Strict`, `Path=/api/auth`.
`POST /api/auth/login`, `/refresh` and `/logout` are the only anonymous endpoints besides `/health`. Every refresh rotates
the token: `NhRefreshTokensRepository.Rotate` revokes the current one and stores its replacement in one transaction with
`update ... where revoked_at is null`, so two concurrent refreshes cannot both win. Presenting a token that was already
rotated (or losing the rotation race) is treated as theft: `RevokeAllOf` closes every session of that user and the answer is
the same generic `InvalidRefreshToken`. Rotate and revoke-all are raw SQL on purpose, so they are not specifications.
`POST /api/auth/logout` revokes the token and clears the cookie.

**Authorization.** The identity always comes from the token: `ICurrentUser` (`HttpContextCurrentUser`) reads `sub` and the
role claim, and no endpoint accepts a `userId` or an `isAdmin` from the client. The fallback policy requires an authenticated
user, so a new endpoint is protected unless it opts out with `AllowAnonymous`. The `Admin` policy requires the admin role,
and `ChatbotMember` is a resource policy: `ChatbotMemberAuthorizationHandler` lets administrators through and otherwise asks
`ChatbotMemberSearcher` (module `ChatbotMembers`, through `IQueryBus`) whether the user belongs to the chatbot named in the
route (`id` or `botKey`). It protects the chatbot detail and the question endpoint, and the chatbot list is filtered by the
same membership. Team roles (owner, editor, reader) exist in the domain but are not enforced yet.

**HTTP statuses.** `ApiExceptionHandler` maps a `DomainError` by its code suffix: `.not_found` 404, `.already_exists` 409,
`.invalid_credentials`, `.invalid_refresh_token` and `.refresh_token_reused` 401, and anything else 422. A failed policy is a
403 from the framework.

**The client keeps the access token in memory only** (`AuthSession`), never in web storage, so a script cannot read it and
the refresh cookie stays unreachable from JavaScript. On load `UserProvider` calls refresh to restore the session
(`isRestoring` holds the UI until it answers). `authFetch` attaches the bearer token and, on a 401 for a non-auth endpoint,
refreshes once and retries; if that fails it expires the session and the user returns to the login.

**Active-users figures.** `AnalyticsFinder.Finder` assembles the whole `AnalyticsResponse` itself instead of returning the
`Analytics` entity, the one deliberate exception to the Finder pattern ([ADR-18](0018-handler-is-a-thin-transformation-layer-the-actual-use-case.md); the `ToResponse` extension is [ADR-21](0021-entity-response-conversion-is-explicit-operator-by-default.md)). The response merges the `Analytics`
singleton's incrementing counters with a live count of users active in the last 7 days
(`UserActiveSinceSpecification`) and the total (`UserListingSpecification`). The rolling window is not part of `Analytics`'s
running totals because an incrementing counter cannot represent users dropping out of "active in the last 7 days" as time
passes; only a live query can.

**Not covered.** Login has no rate limit or lockout, the provider API keys are stored in plain text, the demo
accounts share a fixed password and there is no HSTS/CSP/CORS policy (all in `TODO.md`).

**Where it applies:** `Users/**`, `Authentication/**`, `ChatbotMembers/**`,
`Shared/Infrastructure/Auth/JwtOptions.cs`, `Shared/Infrastructure/Persistence/Nh/Mappings/{UserMapping,RefreshTokenMapping}.cs`,
`Shared/Infrastructure/Persistence/Migrations/InitialSchemaMigration.cs`, `Analytics/Application/Finder/AnalyticsFinder.cs`,
`apps/backend/api/{Configuration/{AuthExtensions,AuthPolicies,ChatbotMemberAuthorizationHandler,ApiExceptionHandler,RefreshTokenCookie,HttpContextCurrentUser,InfrastructureExtensions},Endpoints/{AuthEndpoints,UsersEndpoints}}.cs`,
and on the frontend `apps/frontend/nakama.web/src/{shared/infrastructure/auth/**,modules/auth/**,views/pages/auth/LoginPage.tsx,views/shared/contexts/UserContext.tsx}`.
