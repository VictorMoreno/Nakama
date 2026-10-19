# ADR-13b: Ids for newly-created aggregates/sub-entities are caller-supplied, not server-generated

- **Status:** Accepted
- **Area:** Domain model notes

`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`, and `ConversationExchangeAppender` (when starting a new
conversation) used to generate their new entity's id server-side via `IUuidGenerator` and return it in a `Response`,
which was the only reason those 4 commands returned anything — every other command in the codebase already returns an
empty `Response`. Since a client-generated GUID v4 has negligible collision probability, the id is now supplied by the
caller instead: the client already knows the id before sending the command, so the command can stay a pure side effect
with an empty `Response`, and `IUuidGenerator`/`GuidUuidGenerator` were deleted as dead code (they had no other
callers). This does introduce a risk that server-generated ids never had: a client could resend or accidentally reuse an
id, and `Save`'s NHibernate `SaveOrUpdate` semantics would silently overwrite the existing row instead of failing. The 3
pure-create handlers (`ChatbotCreator`, `FormationCreator`, `FormationModuleAdder`) guard against this explicitly with a
`FindById`-then-throw-`AlreadyExists` check before creating. `ConversationExchangeAppender` is the one exception:
finding an existing row for the supplied id is its *intended* "continue this conversation" path, not a collision, so it
has no such guard — it's a legitimate find-or-start.

**Where it applies:** `Chatbots/Application/Creator/ChatbotCreator.cs`,
`Formations/Application/Creator/FormationCreator.cs`, `Formations/Application/ModuleAdder/FormationModuleAdder.cs`,
`Chatbots/Application/ExchangeAppender/ConversationExchangeAppender.cs`, `Chatbots/Domain/Errors/ChatbotErrors.cs`,
`Formations/Domain/Errors/FormationErrors.cs`
