using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Errors;

public sealed class FormationEnrollmentNotFoundError(string formationId, string userId)
    : DomainError("nakama.formation_enrollment.not_found", $"User '{userId}' is not enrolled in formation '{formationId}'.");

public sealed class FormationEnrollmentExamMustBeSubmittedError(string moduleId)
    : DomainError("nakama.formation_enrollment.exam_must_be_submitted", $"El módulo '{moduleId}' es un examen: se completa entregándolo, no marcándolo como completado.");

public sealed class FormationEnrollmentModuleIsNotAnExamError(string moduleId)
    : DomainError("nakama.formation_enrollment.module_is_not_an_exam", $"El módulo '{moduleId}' no es un examen.");

public sealed class FormationEnrollmentExamAlreadyPassedError(string moduleId)
    : DomainError("nakama.formation_enrollment.exam_already_passed", $"El examen '{moduleId}' ya está aprobado.");

public sealed class FormationEnrollmentInvalidExamAnswersError()
    : DomainError("nakama.formation_enrollment.invalid_exam_answers", "Hay que contestar cada pregunta del examen con una de sus opciones.");

public static class FormationEnrollmentErrors
{
    public static DomainError InvalidExamAnswers { get; } = new FormationEnrollmentInvalidExamAnswersError();

    public static DomainError ExamMustBeSubmitted(Guid moduleId) => new FormationEnrollmentExamMustBeSubmittedError(moduleId.ToString());

    public static DomainError ModuleIsNotAnExam(Guid moduleId) => new FormationEnrollmentModuleIsNotAnExamError(moduleId.ToString());

    public static DomainError ExamAlreadyPassed(Guid moduleId) => new FormationEnrollmentExamAlreadyPassedError(moduleId.ToString());

    public static DomainError NotEnrolled(Guid formationId, Guid userId) =>
        new FormationEnrollmentNotFoundError(formationId.ToString(), userId.ToString());
}
