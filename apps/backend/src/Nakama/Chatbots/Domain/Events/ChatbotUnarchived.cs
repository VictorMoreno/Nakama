using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotUnarchived(ChatbotId Id) : IDomainEvent;
