using Nakama.Formations.Application.DetailsUpdater;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

public static class FormationDetailsUpdaterCommandMother
{
    public static FormationDetailsUpdater.Command Random() => With();

    public static FormationDetailsUpdater.Command With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null) =>
        new(
            id ?? FormationMother.RandomId().Value,
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random());
}
