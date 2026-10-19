# ADR-3: Generic value-object persistence via reflection

- **Status:** Accepted
- **Area:** Persistence

Identity value objects are positional record structs implementing either `IStringValueObject` (`Value: string`) or
`IGuidValueObject` (`Value: Guid`). `ValueObjectUserType<T>`/`GuidValueObjectUserType<T>` (siblings) map any such type
generically onto a varchar/native-uuid column respectively, round-tripping through the type's single-argument
constructor via `Activator.CreateInstance` — this avoids writing a bespoke NHibernate `IUserType` per identity type.

**Where it applies:** `Shared/Domain/{IStringValueObject,IGuidValueObject}.cs`,
`Shared/Infrastructure/Persistence/Nh/UserTypes/{ValueObjectUserType,GuidValueObjectUserType}.cs`
