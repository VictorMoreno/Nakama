using Nakama.FormationDetail.Application.Finder;

namespace Test.Modules.FormationDetail.Application;

public static class FormationDetailFinderQueryMother
{
    public static FormationDetailFinder.Query Random() => With();

    public static FormationDetailFinder.Query With(Guid? id = null) => new(id ?? Guid.NewGuid());
}
