using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotSettingsUpdated(ChatbotId Id, ChatbotSettings Settings) : IDomainEvent;
