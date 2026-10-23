using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ConversationNotFoundError(string key)
    : DomainError("nakama.conversations.not_found", $"Conversation '{key}' not found.");

public sealed class ConversationExchangeNotFoundError(string key, int index)
    : DomainError("nakama.conversations.exchange.not_found", $"Exchange {index} of conversation '{key}' not found.");

public static class ConversationErrors
{
    public static DomainError NotFound(ConversationId id) => new ConversationNotFoundError(id.ToString());

    public static DomainError ExchangeNotFound(ConversationId id, int index) => new ConversationExchangeNotFoundError(id.ToString(), index);
}
