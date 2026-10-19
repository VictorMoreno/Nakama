# ADR-16: Specific business invariants worth flagging

- **Status:** Accepted
- **Area:** Domain model notes

- `Chatbot` protects two invariants: an owner can't be removed from the team (`ChatbotTeamMemberRemover`/
  `Chatbot.RemoveTeamMember` throw `ChatbotErrors.OwnerCannotBeRemoved` when the target index is the owner), and status
  transitions/archival are explicit, event-raising operations rather than free property sets.
- `Chatbot.Context` (HTML shown in the chat panel) is a distinct property from `Chatbot.Description` (the short listing
  blurb) — not duplicates.
- `Formation.domain.cs`'s module-completion path throws `FormationErrors.ModuleLockedByGating` when the previous module
  has gating enabled and hasn't been completed yet (mirrored by `FormationModuleCompleter`).
- `ConversationExchangeAppender`: when `ConversationId` is null, a new `Conversation` is started for the given chatbot —
  this command is the single write-side entry point for both "first message in a new chat" and "reply within an existing
  conversation".
- `LearnerProgress` is embedded in the `Formation` aggregate for the current scope (the frontend models a single demo
  learner); a real multi-user system should promote it to its own `LearnerFormationProgress` aggregate keyed by
  `userId`+`formationKey`, to avoid serializing unrelated learners' writes against the same lock. It's immutable —
  `WithModuleCompleted` returns a new instance rather than mutating.
- `FormationModule`: which fields are relevant depends on `Type` (video uses `ResourceUrl`+`DurationMin`,
  document/exercise use `ResourceUrl`, exam uses `Quiz`+`PassingScore`). It's immutable — `Formation.ToggleModuleGating`
  replaces the list entry with a `with`-copy rather than mutating a field on it.
- `AggregateRoot` (non-generic) is split from `AggregateRoot<TId>` so infrastructure code (`IEventBus`) can accept a
  heterogeneous batch of aggregates (`IEnumerable<AggregateRoot>`) without being tied to a single identity type.

**Where it applies:** `Chatbots/Domain/Chatbot.cs`,
`Chatbots/Application/TeamMemberRemover/ChatbotTeamMemberRemover.cs`, `Formations/Domain/Formation.domain.cs`,
`Formations/Application/ModuleCompleter/FormationModuleCompleter.cs`,
`Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Formations/Domain/LearnerProgress.cs`,
`Formations/Domain/FormationModule.cs`, `Shared/Domain/AggregateRoot.cs`
