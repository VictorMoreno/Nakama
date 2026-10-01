using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleRemoved(FormationId Id, ModuleId ModuleId) : IDomainEvent;
