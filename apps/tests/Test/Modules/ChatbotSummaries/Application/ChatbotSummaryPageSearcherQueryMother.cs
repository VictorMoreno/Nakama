using Nakama.ChatbotSummaries.Application.PageSearcher;

namespace Test.Modules.ChatbotSummaries.Application;

public static class ChatbotSummaryPageSearcherQueryMother
{
    public static ChatbotSummaryPageSearcher.Query Random() => With();

    public static ChatbotSummaryPageSearcher.Query Admin(Guid? userId = null) => With(userId, true);

    public static ChatbotSummaryPageSearcher.Query With(
        Guid? userId = null,
        bool? isAdmin = null,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null) =>
        new(userId ?? Guid.NewGuid(), isAdmin ?? false, page ?? 1, pageSize, search, status);
}
