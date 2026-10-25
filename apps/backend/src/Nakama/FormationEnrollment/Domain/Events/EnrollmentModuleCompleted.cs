using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Events;

public sealed record EnrollmentModuleCompleted(Guid FormationId, Guid UserId, Guid ModuleId, int? Score, int CompletedModuleCount) : IDomainEvent;
