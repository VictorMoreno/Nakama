using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotDocumentAdded(ChatbotId Id, ChatbotDocument Document) : IDomainEvent;
