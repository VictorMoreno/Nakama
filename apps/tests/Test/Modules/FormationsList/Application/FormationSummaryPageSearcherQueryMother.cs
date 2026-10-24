using Nakama.FormationsList.Application.PageSearcher;

namespace Test.Modules.FormationsList.Application;

public static class FormationSummaryPageSearcherQueryMother
{
    public static FormationSummaryPageSearcher.Query Random() => With();

    public static FormationSummaryPageSearcher.Query Admin(Guid? userId = null) => With(userId, true);

    public static FormationSummaryPageSearcher.Query With(
        Guid? userId = null,
        bool? isAdmin = null,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null,
        string? progress = null) =>
        new(userId ?? Guid.NewGuid(), isAdmin ?? false, page ?? 1, pageSize, search, status, Progress: progress);
}
