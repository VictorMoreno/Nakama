# ADR-11: Analytics defensive dual-path handler

- **Status:** Accepted
- **Area:** Domain events

`Analytics` has no dedicated creation event of its own — it's fed by whichever of several event types (`ChatbotCreated`,
`ConversationExchangeAppended`, ...) arrives first. Its handlers are therefore defensive: on the first feeding event to
arrive, create the row via a `CreateFromX` factory and `Save`; if the row already exists (some other feeding event got
there first), apply the event onto it and `Update` instead.

**Where it applies:**
`Analytics/Application/Handlers/{AnalyticsCreatedOnChatbotCreated,AnalyticsCreatedOnConversationExchangeAppended}.cs`
