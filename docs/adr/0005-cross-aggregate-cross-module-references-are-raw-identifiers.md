# ADR-5: Cross-aggregate/cross-module references are raw identifiers, never the other side's Domain type

- **Status:** Accepted
- **Area:** Persistence

`Conversation` references `Chatbot` only by `ChatbotId` value, never by object reference or NHibernate association —
a bounded context must not reference another context's Domain (agents.md §1), and `Conversation` is deliberately its
own aggregate (see [ADR-9](0009-denormalized-read-model-entities-pattern.md) for why). Mapping-wise, `ConversationMapping` declares it as a plain scalar column,
never an NHibernate association, for the same reason. `FormationEnrollment` holds `FormationId` and `UserId`, and
`ChatbotMember` holds `ChatbotId` and `UserId`, as plain `Guid`s too.

**Where it applies:** `Chatbots/Domain/Conversation.cs`, `FormationEnrollment/Domain/FormationEnrollment.cs`,
`ChatbotMembers/Domain/ChatbotMember.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/{ConversationMapping,FormationEnrollmentMapping,ChatbotMembersMapping}.cs`
