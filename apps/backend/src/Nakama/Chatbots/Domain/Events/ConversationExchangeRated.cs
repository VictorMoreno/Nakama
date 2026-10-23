using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationExchangeRated(
    ConversationId Id,
    ChatbotId BotKey,
    Guid UserId,
    int ExchangeIndex,
    string Question,
    bool Useful,
    DateTime OccurredAt) : IDomainEvent;
