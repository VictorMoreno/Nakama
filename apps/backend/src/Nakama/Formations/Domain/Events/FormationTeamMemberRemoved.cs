using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationTeamMemberRemoved(FormationId Id, int Index) : IDomainEvent;
