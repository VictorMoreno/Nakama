# ADR-16: Specific business invariants worth flagging

- **Status:** Accepted
- **Area:** Domain model notes

- `Chatbot` protects two invariants: an owner can't be removed from the team (`Chatbot.RemoveTeamMember`, reached through
  `ChatbotTeamMemberRemover`, throws `ChatbotErrors.OwnerCannotBeRemoved` when the target member is the owner; the member is identified by user id, never by list position), and status
  transitions and archival are explicit, event-raising operations rather than free property sets.
- `ChatbotDetail.Context` (HTML shown in the chat panel) is a distinct property from `Description` (the short listing
  blurb) — not duplicates.
- `FormationModuleCompleter` (module `FormationEnrollment`) throws `FormationErrors.ModuleLockedByGating` when the
  previous module has gating enabled and hasn't been completed yet.
- `ChatbotQuestionAsker` is the single write-side entry point for both "first message in a new chat" and "reply within an
  existing conversation": when no conversation exists for the supplied id it starts a new `Conversation` for the given
  chatbot.
- `LearnerProgress` lives inside the `FormationEnrollment` aggregate, one per formation and user (unique index on
  `user_id, formation_id`), so unrelated learners' writes never serialize against the same row. It's immutable —
  `WithModuleCompleted` returns a new instance rather than mutating.
- `FormationModule`: which fields are relevant depends on `Type` (video uses `ResourceUrl`+`DurationMin`,
  document/exercise use `ResourceUrl`, exam uses `Quiz`+`PassingScore`). It's immutable — `Formation.ToggleModuleGating`
  replaces the list entry with a `with`-copy rather than mutating a field on it.
- `AggregateRoot` (non-generic) is split from `AggregateRoot<TId>` so infrastructure code (`IEventBus`) can accept a
  heterogeneous batch of aggregates without being tied to a single identity type.

**Where it applies:** `Chatbots/Domain/Chatbot.domain.cs`,
`Chatbots/Application/TeamMemberRemover/ChatbotTeamMemberRemover.cs`, `ChatbotDetail/Domain/ChatbotDetail.cs`,
`FormationEnrollment/Application/ModuleCompleter/FormationModuleCompleter.cs`,
`Chatbots/Application/QuestionAsker/ChatbotQuestionAsker.cs`, `FormationEnrollment/Domain/LearnerProgress.cs`,
`Formations/Domain/{Formation.domain,FormationModule}.cs`, `Shared/Domain/AggregateRoot.cs`
