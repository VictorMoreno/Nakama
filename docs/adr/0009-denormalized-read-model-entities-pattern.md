# ADR-9: Denormalized read-model entities pattern

- **Status:** Accepted
- **Area:** Domain events

A read entity (`ChatbotSummary`, `ChatbotDetail`, `FormationSummary`, `FormationDetail`, `Analytics`)
implements `IEntity` like any domain entity but is **not** an `AggregateRoot`: it never raises its own events, it only
consumes events raised by a real aggregate, and it's fed exclusively through its own module's `Application/Handlers`.
One read model exists per route, not per tab/section — e.g. `ChatbotDetail` covers all 4 tabs (General, Documents, Team,
Analytics) of `/chatbots/:key` in one entity, including a cross-aggregate derivation
(`Apply(ConversationExchangeAppended)` computes the Analytics tab's "most repeated questions" from `Conversation`
events). Where no real signal exists yet for a field (e.g. `ChatbotDetail.Gaps`, "was this answer good"), it's
deliberately left empty rather than faking a heuristic. `Analytics` is the degenerate case: not an aggregate, no identity of its own, a singleton row (`SingletonKey` is
always `"global"`) computed entirely as a derived projection across `Chatbot`/`Conversation`/`Formation` events — see
[ADR-11](0011-analytics-defensive-dual-path-handler.md) for its specific defensive-handler shape.

**Where it applies:** `ChatbotSummaries/Domain/ChatbotSummary.cs`, `ChatbotDetail/Domain/ChatbotDetail.cs`,
`FormationsList/Domain/FormationSummary.cs`, `FormationDetail/Domain/FormationDetail.cs`,
`Analytics/Domain/Analytics.cs`
