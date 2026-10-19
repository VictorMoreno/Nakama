# ADR-10: Cross-module event subscriptions are intentional projection coupling

- **Status:** Accepted
- **Area:** Domain events

A read-model handler may subscribe to a *different* module's `Domain/Events` — for example
`FormationDetailCreatedOnFormationCreated` listens to `Formations`' `FormationCreated`. That is the intentional
projection coupling this pattern relies on: the read model never touches the write side's Domain entities or another
read model's repository, it only consumes the events. The `ChatbotLabelRef` lookup that once resolved a linked
chatbot's label for `FormationDetail` was removed together with the formation's linked chatbot, since nothing consumed it; neither the column nor the
table exists in the baseline schema ([ADR-19](0019-fluentmigrator-replaces-dbup-as-the-migration-runner-every.md)).

**Where it applies:** `FormationDetail/Application/Handlers/FormationDetailCreatedOnFormationCreated.cs`,
`ChatbotSummaries/Application/Handlers/*`
