using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationStarted(ConversationId Id, ChatbotId BotKey, Guid UserId, string Title) : IDomainEvent;
