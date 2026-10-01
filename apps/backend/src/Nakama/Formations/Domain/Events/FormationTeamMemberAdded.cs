using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationTeamMemberAdded(FormationId Id, FormationMember Member) : IDomainEvent;
