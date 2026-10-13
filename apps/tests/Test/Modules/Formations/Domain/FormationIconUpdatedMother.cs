using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationIconUpdatedMother
{
    public static FormationIconUpdated Random() => With();

    public static FormationIconUpdated Cleared(FormationId? id = null) => new(id ?? FormationMother.RandomId(), null);

    public static FormationIconUpdated With(
        FormationId? id = null,
        string? iconUrl = null) =>
        new(
            id ?? FormationMother.RandomId(),
            iconUrl ?? $"{WordMother.Random()}.png");
}
