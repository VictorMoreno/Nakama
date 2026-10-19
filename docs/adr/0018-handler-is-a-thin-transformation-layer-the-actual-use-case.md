# ADR-18: `Handler` is a thin transformation layer; the actual use case is a separate, Scrutor-scanned class — Commands and Queries alike

- **Status:** Accepted
- **Area:** CQRS / cross-module reads

Every Command's `Handler.Handle` does nothing but convert every primitive `Command` field into its domain-typed value —
identity value objects (`ChatbotId.From(cmd.Id)`), enums (`(ChatbotStatus)cmd.Status`), and domain records
(`new ChatbotSettings(cmd.Socratic, cmd.ExplicaMas, (LlmProvider)cmd.Provider)`) alike, no exceptions for "just" an
enum — and delegate to a second nested class whose `Execute` signature is therefore *fully domain-typed*, never touching
a raw `Command` field or doing a cast itself. That second class does the real work
(find-or-throw/find-or-create/find-or-start, mutate, `Save`, `Publish`). Every Query's `Handler.Handle` mirrors this
from the other side: the second nested class does the real work (fetch-or-throw `NotFound`, or `FindAll`), and `Handler`
holds only the output transformation (the `(Response)entity` cast, or the `.Select(e => (Response)e).ToList()`
projection for a list). That second class implements `IService` (`Shared/Application/IService.cs`) — an empty marker
interface whose only purpose is to be a Scrutor scan target (`InfrastructureExtensions.cs`'s `AddApplication()`,
`classes.AssignableTo<IService>().AsSelf()`). It's registered `AsSelf()`, not `AsImplementedInterfaces()`, deliberately:
every `Handler` injects the *concrete* use-case class (`Creator`, `Adder`, `Remover`, `Updater`, `Archiver`, `Toggler`,
`Completer`, `Appender`, `Finder`, `Searcher`...), never `IService` itself — dozens of unrelated classes share that one
marker, so resolving by the interface would be ambiguous.

**Naming the nested class**: it is named after what it does, matching the outer static class's own suffix —
`ChatbotCreator.Creator`, `ChatbotDocumentAdder.Adder`, `ChatbotDocumentRemover.Remover`,
`ChatbotStatusUpdater.Updater`, `ConversationExchangeAppender.Appender`, `FormationModuleGatingToggler.Toggler`,
`FormationModuleCompleter.Completer`, `ChatbotDetailFinder.Finder`,
`FormationDetailFinder.Finder`, `AnalyticsFinder.Finder`, `ChatbotSummarySearcher.Searcher`,
`FormationSummarySearcher.Searcher`. It is *not* always called `Updater` — that name is reserved for cases that are
genuinely an update of an existing aggregate's field (`ChatbotStatusUpdater`, `ChatbotSettingsUpdater`,
`ChatbotIconUpdater`, `FormationStatusUpdater`, `FormationIconUpdater`).

**When to use a `<Aggregate>FinderById` domain service** (`Chatbots/Domain/Services/ChatbotFinderById.cs`,
`Formations/Domain/Services/FormationFinderById.cs`) inside the use-case class, vs. not:

- **Find existing-or-throw `NotFound`, then mutate** (most commands: `Archiver`, `Updater` variants, `Adder`/`Remover`
  variants acting on an existing aggregate): use the finder — it centralizes
  `repository.FindById(id, ct) ?? throw <Aggregate>Errors.NotFound(id)` in one place per aggregate instead of repeating
  it in every use case.
- **Check the aggregate does NOT already exist, then create it** (`ChatbotCreator`, `FormationCreator`): the inverse
  check — a `NotFound`-throwing finder doesn't fit, keep the `FindById(...) is not null` check inline.
- **Find-or-implicitly-create (upsert), never throws** (`ConversationExchangeAppender`): doesn't fit a throwing finder
  either, keep the `FindById(...) ?? Start(...)` inline.
- **A sub-resource-level check inside an aggregate already found via the finder** (`FormationModuleAdder`'s "module must
  not already exist" check, after finding the *Formation* via `FormationFinderById`): stays inline, same reasoning as
  the Creators — it's a single caller, not worth its own service class.

**Queries don't get a `Domain/Services` finder of their own.** `ChatbotFinderById`/`FormationFinderById` exist because
the *same* "find this aggregate or throw `NotFound`" logic is genuinely repeated across many different Commands for the
same aggregate. Every read-model Query (`ChatbotDetailFinder`, `FormationDetailFinder`,
`AnalyticsFinder`, and the two Searchers) has exactly one consumer — its own `Handler` — so the `Finder`/`Searcher`
nested class already *is* the use case; there's no second caller to justify pulling the fetch logic out into a shared
`Domain/Services` class the way the write side does.

**Where it applies:** `Shared/Application/IService.cs`, every `<UseCase>.Handler`/`<UseCase>.<Action>` pair under
`**/Application/*/`, `Chatbots/Domain/Services/ChatbotFinderById.cs`,
`Formations/Domain/Services/FormationFinderById.cs`, the `IService` Scrutor scan in
`apps/backend/api/Configuration/InfrastructureExtensions.cs`.
