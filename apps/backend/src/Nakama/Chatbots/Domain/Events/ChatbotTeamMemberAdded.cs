using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Events;

public sealed record ChatbotTeamMemberAdded(ChatbotKey Key, TeamMember Member) : IDomainEvent;
