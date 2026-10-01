using Nakama.ConversationHistory.Domain;

namespace Nakama.ConversationHistory.Application;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed record ConversationHistoryResponse(Guid BotKey, List<ConversationSummaryRow> Entries)
{
    public static explicit operator ConversationHistoryResponse(ConversationHistory entity) => new(
        entity.Id,
        entity.Entries);
}
