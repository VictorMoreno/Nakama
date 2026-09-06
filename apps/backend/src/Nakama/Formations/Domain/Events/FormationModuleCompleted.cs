using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleCompleted(FormationKey Key, ModuleId ModuleId, int? Score) : IDomainEvent;
