using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationStatusChanged(FormationKey Key, FormationStatus Status) : IDomainEvent;
