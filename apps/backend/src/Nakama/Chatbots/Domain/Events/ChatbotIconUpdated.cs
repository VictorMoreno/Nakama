using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotIconUpdated(ChatbotId Id, string? IconUrl) : IDomainEvent;
