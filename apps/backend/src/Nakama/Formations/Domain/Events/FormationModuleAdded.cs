using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleAdded(FormationKey Key, FormationModule Module) : IDomainEvent;
