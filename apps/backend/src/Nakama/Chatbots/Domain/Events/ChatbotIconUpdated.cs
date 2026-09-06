using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotIconUpdated(ChatbotKey Key, string? IconUrl) : IDomainEvent;
