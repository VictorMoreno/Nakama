# ADR-10: Cross-module event subscriptions are intentional projection coupling

- **Status:** Accepted
- **Area:** Domain events

A read-model handler may subscribe to a *different* module's `Domain/Events`: `FormationDetailCreatedOnFormationCreated`
listens to `Formations`' `FormationCreated`, and `ChatbotMemberCreatedOnChatbotTeamMemberAdded` (module `ChatbotMembers`)
listens to `Chatbots`' `ChatbotTeamMemberAdded`. That is the intentional projection coupling this pattern relies on: the
read model never touches the write side's Domain entities or another read model's repository, it only consumes the
events. When a module needs a value that another module owns *right now*, it asks a query instead
([ADR-12](0012-iquerybus-for-live-cross-module-reads-denormalized-entities.md)).

**Where it applies:** `FormationDetail/Application/Handlers/FormationDetailCreatedOnFormationCreated.cs`,
`ChatbotMembers/Application/Handlers/*`, `ChatbotSummaries/Application/Handlers/*`
