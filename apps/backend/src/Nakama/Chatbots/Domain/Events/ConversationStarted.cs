using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ConversationStarted(ConversationId Id, ChatbotKey BotKey, string Title) : IDomainEvent;
