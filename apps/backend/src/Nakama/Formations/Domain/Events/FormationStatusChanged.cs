using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationStatusChanged(FormationId Id, FormationStatus Status) : IDomainEvent;
