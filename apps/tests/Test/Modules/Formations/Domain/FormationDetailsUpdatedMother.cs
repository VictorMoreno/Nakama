using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationDetailsUpdatedMother
{
    public static FormationDetailsUpdated Random() => With();

    public static FormationDetailsUpdated With(
        FormationId? id = null,
        string? title = null,
        string? audience = null,
        string? description = null) =>
        new(
            id ?? FormationMother.RandomId(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random());
}
