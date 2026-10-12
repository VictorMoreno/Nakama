using Nakama.Formations.Application.StatusUpdater;
using Nakama.Formations.Domain;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationStatusUpdaterCommandMother
{
    public static FormationStatusUpdater.Command Random() => With();

    public static FormationStatusUpdater.Command With(Guid? id = null, FormationStatus? status = null) =>
        new(id ?? FormationMother.RandomId().Value, (int)(status ?? FormationStatus.Active));
}
