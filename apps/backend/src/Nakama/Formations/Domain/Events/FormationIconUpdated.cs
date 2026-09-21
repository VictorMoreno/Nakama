using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationIconUpdated(FormationId Id, string? IconUrl) : IDomainEvent;
