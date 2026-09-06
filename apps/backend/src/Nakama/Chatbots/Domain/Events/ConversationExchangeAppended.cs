using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationExchangeAppended(ConversationId Id, ChatbotKey BotKey, ConversationExchange Exchange) : IDomainEvent;
