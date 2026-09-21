using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotCreated(
    ChatbotId Id,
    string Label,
    string Description,
    ChatbotStatus Status,
    bool General,
    ChatbotSettings Settings) : IDomainEvent;