using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationUnarchived(FormationId Id) : IDomainEvent;
