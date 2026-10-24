namespace Nakama.FormationEnrollment.Domain;

public sealed record ExamAttempt(Guid ModuleId, int Score, bool Passed, DateTime AttemptedAt);
