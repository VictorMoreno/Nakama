# ADR-15: Several Domain enums intentionally mirror a frontend TypeScript union

- **Status:** Accepted
- **Area:** Domain model notes

`ModuleType`, `FormationStatus`, `ChatbotStatus`, `LlmProvider`, `DocStatus` each mirror an equivalent frontend union
type, kept in sync by convention (not shared code) since these are two separate stacks. `TeamRole` is a partial mirror:
the frontend distinguishes grammatical gender for the owner label ("Propietario"/"Propietaria") for display purposes
only — the domain models a single `Owner` role, rendering the gendered label is a presentation concern. `TeamMember`'s
`isOwnerRole`-equivalent check is the domain invariant carrier behind that same frontend concept.

**Where it applies:** `Chatbots/Domain/{ChatbotStatus,LlmProvider,DocStatus,TeamRole,TeamMember}.cs`,
`Formations/Domain/{FormationStatus,ModuleType}.cs`
