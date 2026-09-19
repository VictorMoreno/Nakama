using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotDocumentRemoved(ChatbotId Id, int Index) : IDomainEvent;
