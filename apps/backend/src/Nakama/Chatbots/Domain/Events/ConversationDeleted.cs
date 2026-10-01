using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationDeleted(ConversationId Id, ChatbotId BotKey) : IDomainEvent;
