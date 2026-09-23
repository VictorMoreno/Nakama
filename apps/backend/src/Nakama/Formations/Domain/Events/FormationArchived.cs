using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationArchived(FormationId Id) : IDomainEvent;
