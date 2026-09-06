using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotStatusChanged(ChatbotKey Key, ChatbotStatus Status) : IDomainEvent;
