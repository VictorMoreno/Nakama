using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleAdded(FormationId Id, FormationModule Module) : IDomainEvent;
