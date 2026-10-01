using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationExchangeAppended(ConversationId Id, ChatbotId BotKey, Guid UserId, ConversationExchange Exchange)
    : IDomainEvent;