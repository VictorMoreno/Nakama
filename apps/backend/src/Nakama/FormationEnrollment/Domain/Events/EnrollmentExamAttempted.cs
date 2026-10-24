using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Events;

public sealed record EnrollmentExamAttempted(Guid FormationId, Guid UserId, Guid ModuleId, int Score, bool Passed, DateTime AttemptedAt) : IDomainEvent;
