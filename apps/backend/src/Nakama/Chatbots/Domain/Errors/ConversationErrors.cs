using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ConversationNotFoundError(string key)
    : DomainError("nakama.conversations.not_found", $"Conversation '{key}' not found.");

public static class ConversationErrors
{
    public static DomainError NotFound(ConversationId id) => new ConversationNotFoundError(id.ToString());
}
