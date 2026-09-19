using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleGatingToggled(FormationId Id, ModuleId ModuleId, bool Gated) : IDomainEvent;
