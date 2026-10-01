using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Errors;

public sealed class FormationEnrollmentNotFoundError(string formationId, string userId)
    : DomainError("nakama.formation_enrollment.not_found", $"User '{userId}' is not enrolled in formation '{formationId}'.");

public static class FormationEnrollmentErrors
{
    public static DomainError NotEnrolled(Guid formationId, Guid userId) =>
        new FormationEnrollmentNotFoundError(formationId.ToString(), userId.ToString());
}
