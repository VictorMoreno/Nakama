using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ConversationNotFoundError(string key)
    : DomainError("nakama.conversations.not_found", $"Conversation '{key}' not found.");

public static class ConversationErrors
{
    // Also used when the conversation exists but belongs to a different user - the 404 doesn't
    // reveal that distinction, same "don't leak existence to the wrong owner" spirit as auth
    // codebases generally follow, even though this app has no real session enforcement yet.
    public static DomainError NotFound(ConversationId id) => new ConversationNotFoundError(id.ToString());
}
