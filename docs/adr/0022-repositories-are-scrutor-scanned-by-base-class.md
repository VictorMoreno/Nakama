# ADR-22: Repositories are Scrutor-scanned by base class, and registered in `AddInfrastructure`, not `AddApplication`

- **Status:** Accepted
- **Area:** Persistence

Every `I<X>Repository`/`Nh<X>Repository` pair used to be bound one line at a time
(`services.AddScoped<IChatbotsRepository, NhChatbotsRepository>();`, repeated for all eleven) inside
`AddApplication()` — wrong method (a repository binding is an Infrastructure concern, not Application), and it meant
every new bounded context needed a manual line added here on top of writing the repository itself.

Both are fixed together. The binding moved to `AddInfrastructure()`, and it's no longer manual: every
`Nh<X>Repository` already derives from `NhRepositoryBase<TEntity, TId>` ([ADR-2](0002-nhrepositorybase-shared-crud-via-inheritance-not-delegation.md)) and implements exactly one custom
`I<X>Repository` interface, so `services.Scan(scan => scan.FromAssemblyOf<Chatbot>().AddClasses(classes =>
classes.AssignableTo(typeof(NhRepositoryBase<,>))).AsImplementedInterfaces().WithScopedLifetime())` finds all eleven
by their shared open-generic base and binds each to whatever single interface it implements — same
`FromAssemblyOf<Chatbot>()` anchor the existing Handler/`IService` scan in `AddApplication()` already uses, since
this is still one assembly. A future bounded context's repository needs zero lines added here: derive from
`NhRepositoryBase<,>`, implement its own interface, done. Verified live (not just a clean build, which reflection-based
DI wiring can pass even when broken) by exercising an endpoint from every bounded context — chatbots, formations,
analytics, chatbot detail, chat page, and login — against a running instance after the change.

**`AssemblyHelper.Domain`** (`Shared/Infrastructure/AssemblyHelper.cs`) replaced the three separate `typeof(Chatbot)`/
`FromAssemblyOf<Chatbot>()` anchors this file used to have (`AddMediatR`, and both `Scan()` calls) with one static
`Assembly` property, so the "which type do we anchor reflection scans on" choice lives in exactly one place. It's
still `typeof(Chatbot).Assembly` under the hood — Domain/Application/Infrastructure are namespaces inside one
physical assembly (`Nakama.csproj`), not separate ones, so `.Domain` here names *the anchor's layer*, not a distinct
assembly. The `Api` host project (genuinely a separate assembly) keeps its own `typeof(Program)` anchor in
`EndpointExtensions.cs`'s `IEndpoint` scan unchanged — `Nakama.csproj` can't reference `Program` (`Api` depends on
`Nakama`, never the reverse), so a single cross-project helper isn't possible here; each project anchors its own
scans. Scrutor's `FromAssembly(Assembly)` doesn't exist — the actual method is `FromAssemblies(params Assembly[])`,
found by letting the compiler error rather than assuming.

**Where it applies:** `apps/backend/api/Configuration/InfrastructureExtensions.cs`,
`Shared/Infrastructure/AssemblyHelper.cs`.
