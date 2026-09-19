using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationModuleCompleted(FormationId Id, ModuleId ModuleId, int? Score) : IDomainEvent;
