using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotDocumentRemoved(ChatbotKey Key, int Index) : IDomainEvent;
