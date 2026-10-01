using Nakama.Shared.Domain;

namespace Nakama.ConversationHistory.Domain.Errors;

public sealed class ConversationHistoryNotFoundError(string botKey)
    : DomainError("nakama.conversation_history.not_found", $"ConversationHistory '{botKey}' not found.");

public static class ConversationHistoryErrors
{
    public static DomainError NotFound(string botKey) => new ConversationHistoryNotFoundError(botKey);
}
