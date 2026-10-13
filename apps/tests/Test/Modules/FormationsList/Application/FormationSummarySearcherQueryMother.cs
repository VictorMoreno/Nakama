using Nakama.FormationsList.Application.Searcher;

namespace Test.Modules.FormationsList.Application;

public static class FormationSummarySearcherQueryMother
{
    public static FormationSummarySearcher.Query Random() => With();

    public static FormationSummarySearcher.Query Admin(Guid? userId = null) => With(userId, true);

    public static FormationSummarySearcher.Query With(Guid? userId = null, bool? isAdmin = null) =>
        new(userId ?? Guid.NewGuid(), isAdmin ?? false);
}
