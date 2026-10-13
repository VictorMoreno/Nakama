using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationStatusChangedMother
{
    public static FormationStatusChanged Random() => With();

    public static FormationStatusChanged With(
        FormationId? id = null,
        FormationStatus? status = null) =>
        new(
            id ?? FormationMother.RandomId(),
            status ?? FormationStatus.Active);
}
