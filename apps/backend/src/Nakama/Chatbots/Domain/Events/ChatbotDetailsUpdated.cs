using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotDetailsUpdated(ChatbotId Id, string Label, string Description) : IDomainEvent;
