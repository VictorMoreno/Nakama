using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationCreated(FormationId Id, string Title, string Audience, string Description, FormationStatus Status) : IDomainEvent;
