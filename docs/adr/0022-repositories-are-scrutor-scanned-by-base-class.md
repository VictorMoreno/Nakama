# ADR-22: Repositories are Scrutor-scanned by base class, and registered in `AddInfrastructure`, not `AddApplication`

- **Status:** Accepted
- **Area:** Persistence

Repositories are bound by one scan in `AddInfrastructure()`, not by hand and not in `AddApplication()` (a repository
binding is an Infrastructure concern, and a manual line per repository would be one more thing to remember for every new
bounded context). Every `Nh<X>Repository` derives from `NhRepositoryBase<TEntity, TId>`
([ADR-2](0002-nhrepositorybase-shared-crud-via-inheritance-not-delegation.md)) and implements exactly one custom
`I<X>Repository` interface, so

```csharp
services.Scan(scan => scan
    .FromAssemblies(AssemblyHelper.Domain)
    .AddClasses(classes => classes.AssignableTo(typeof(NhRepositoryBase<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

finds all of them through their shared open-generic base and binds each to the interface it implements. A new bounded
context's repository needs no registration line: derive from the base and implement your interface. The one exception is
the Identity store `NhUserRepository`, which is not an `NhRepositoryBase` and is registered with
`AddIdentityCore<User>().AddUserStore<NhUserRepository>()` ([ADR-20](0020-users-and-authentication-asp-net-core-identity-core.md)).
Reflection-based wiring can pass a clean build and still fail at resolution time, so the E2E tests, which resolve the
services behind every endpoint, are what proves it.

**`AssemblyHelper.Domain`** (`Shared/Infrastructure/AssemblyHelper.cs`) is the one place that decides which type anchors
the reflection scans: `typeof(Chatbot).Assembly`, used by `AddMediatR` and by every `Scan()` in `AddInfrastructure()` and
`AddApplication()`. Domain, Application and Infrastructure are namespaces of one physical assembly (`Nakama.csproj`), so
`.Domain` names the layer of the anchor type, not a separate assembly. The `Api` host is a different assembly and anchors
its `IEndpoint` scan on `FromAssemblyOf<Program>()` in `EndpointExtensions.cs`: `Nakama` cannot reference `Program`
(`Api` depends on `Nakama`, never the reverse), so each project anchors its own scans. Scrutor has no `FromAssembly`; the
method is `FromAssemblies(params Assembly[])`.

**Where it applies:** `apps/backend/api/Configuration/{InfrastructureExtensions,EndpointExtensions}.cs`,
`Shared/Infrastructure/AssemblyHelper.cs`.
