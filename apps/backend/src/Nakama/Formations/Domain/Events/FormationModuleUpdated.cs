using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleUpdated(FormationId Id, FormationModule Module) : IDomainEvent;
