# ADR-13b: Ids for newly-created aggregates/sub-entities are caller-supplied, not server-generated

- **Status:** Accepted
- **Area:** Domain model notes

A resource that a client creates and then refers to has its id supplied by the caller (a client-generated GUID v4); the
server does not generate it. That keeps the command a pure side effect with an empty result, since the client already
knows the id it needs, and there is no id-generator abstraction. `ChatbotCreator`, `FormationCreator` and
`FormationModuleAdder` take the id from their command.

Supplying ids brings a risk that server-generated ids never had: a client can resend or reuse an id, and `Save`'s
`SaveOrUpdate` semantics would silently overwrite the existing row instead of failing. The three pure-create use cases
therefore check first and throw `AlreadyExists` (`ChatbotErrors.AlreadyExists`, `FormationErrors.AlreadyExists`, and
`FormationErrors.ModuleAlreadyExists` for a module that is already in its formation).

`ChatbotQuestionAsker` is a find-or-start rather than a create. The conversation id in the route is either the caller's own
conversation, to continue it, or an unused one, to start it. `ConversationFinderById.FindOwned` throws `NotFound` when the
id belongs to another user, so an id cannot be used to take over or overwrite someone else's conversation.

Aggregates that exist as a side effect of another action get a server-generated GUID, because no client ever needs to
name them in advance: `RefreshToken` (`RefreshTokenId.New()` at login or refresh), `FormationEnrollment` (created when a
member is added to a formation) and `LlmProviderConfig` (registered on first use of a provider).

**Where it applies:** `Chatbots/Application/Creator/ChatbotCreator.cs`,
`Formations/Application/Creator/FormationCreator.cs`, `Formations/Application/ModuleAdder/FormationModuleAdder.cs`,
`Chatbots/Application/QuestionAsker/ChatbotQuestionAsker.cs`, `Chatbots/Domain/Services/ConversationFinderById.cs`,
`Chatbots/Domain/Errors/ChatbotErrors.cs`, `Formations/Domain/Errors/FormationErrors.cs`
