# ADR-4: Complex nested values persist as a single JSON column

- **Status:** Accepted
- **Area:** Persistence

`JsonUserType<T>` persists any complex .NET value (a list, a record, a nested value-object graph — e.g. the quiz
embedded in a formation module, or a chatbot's documents/team lists) as one JSON text column, per agents.md §6. This
avoids modelling every nested collection as its own NHibernate component/bag mapping. Used for `Chatbot.Settings` and
the private `_documents`/`_team` backing fields (`ChatbotMapping`).

**Where it applies:** `Shared/Infrastructure/Persistence/Nh/UserTypes/JsonUserType.cs`,
`Shared/Infrastructure/Persistence/Nh/Mappings/ChatbotMapping.cs`
