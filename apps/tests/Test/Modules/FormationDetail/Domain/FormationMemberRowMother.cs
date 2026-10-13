using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Domain;

public static class FormationMemberRowMother
{
    public static FormationMemberRow Random() => With();

    public static FormationMemberRow With(Guid? userId = null, int? progress = null) =>
        new() { UserId = userId ?? Guid.NewGuid(), Progress = progress ?? MotherCreator.Random.Random.Number(0, 100) };
}
