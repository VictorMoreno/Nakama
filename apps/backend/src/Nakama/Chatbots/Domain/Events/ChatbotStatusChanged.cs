using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotStatusChanged(ChatbotId Id, ChatbotStatus Status) : IDomainEvent;
