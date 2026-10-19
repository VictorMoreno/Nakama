using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotDocumentRemoved(ChatbotId Id, Guid DocumentId) : IDomainEvent;
