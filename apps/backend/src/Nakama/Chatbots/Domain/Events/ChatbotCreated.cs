using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotCreated(
    ChatbotKey Key,
    string Label,
    string Description,
    ChatbotStatus Status,
    bool General) : IDomainEvent;