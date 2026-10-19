using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationCreatedMother
{
    public static FormationCreated Random() => With();

    public static FormationCreated With(
        FormationId? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        FormationStatus? status = null) =>
        new(
            id ?? FormationMother.RandomId(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? FormationStatus.Active);
}
