using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationDetailsUpdated(FormationId Id, string Title, string Audience, string Description) : IDomainEvent;
