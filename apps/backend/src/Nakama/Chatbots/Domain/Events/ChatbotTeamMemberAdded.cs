using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotTeamMemberAdded(ChatbotId Id, TeamMember Member) : IDomainEvent;
