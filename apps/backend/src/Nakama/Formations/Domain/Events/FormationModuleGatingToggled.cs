using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleGatingToggled(FormationKey Key, ModuleId ModuleId, bool Gated) : IDomainEvent;
