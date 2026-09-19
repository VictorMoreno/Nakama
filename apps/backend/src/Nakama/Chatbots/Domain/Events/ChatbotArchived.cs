using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotArchived(ChatbotId Id) : IDomainEvent;
