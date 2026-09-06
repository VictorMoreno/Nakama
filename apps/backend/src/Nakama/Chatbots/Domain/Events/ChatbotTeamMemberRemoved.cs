using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotTeamMemberRemoved(ChatbotKey Key, int Index) : IDomainEvent;
