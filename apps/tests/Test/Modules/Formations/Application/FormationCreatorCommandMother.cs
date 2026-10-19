using Nakama.Formations.Application.Creator;
using Nakama.Formations.Domain;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

public static class FormationCreatorCommandMother
{
    public static FormationCreator.Command Random() => With();

    public static FormationCreator.Command With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        FormationStatus? status = null) =>
        new(
            id ?? FormationMother.RandomId().Value,
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            (int)(status ?? FormationStatus.Active));
}
