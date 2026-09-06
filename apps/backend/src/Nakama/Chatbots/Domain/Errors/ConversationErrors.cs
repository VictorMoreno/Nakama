using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ConversationNotFoundError(string id)
    : DomainError("nakama.chatbots.conversation_not_found", $"Conversation '{id}' not found.");

public static class ConversationErrors
{
    public static DomainError NotFound(string id) => new ConversationNotFoundError(id);
}
