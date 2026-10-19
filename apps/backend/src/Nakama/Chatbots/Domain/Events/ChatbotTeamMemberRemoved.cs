using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotTeamMemberRemoved(ChatbotId Id, int Index, Guid UserId) : IDomainEvent;
