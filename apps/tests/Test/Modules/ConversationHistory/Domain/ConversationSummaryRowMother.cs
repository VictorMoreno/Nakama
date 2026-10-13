using Nakama.ConversationHistory.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationHistory.Domain;

public static class ConversationSummaryRowMother
{
    public static ConversationSummaryRow Random() => With();

    public static ConversationSummaryRow With(
        Guid? id = null,
        Guid? botKey = null,
        Guid? userId = null,
        string? title = null,
        DateTime? dateLabel = null,
        List<HistoryExampleRow>? exchanges = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            BotKey = botKey ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Title = title ?? WordMother.Random(),
            DateLabel = dateLabel ?? DateTime.UtcNow,
            Exchanges = exchanges ?? []
        };
}
