using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain;

public partial class FormationEnrollment
{
    public FormationEnrollment(FormationEnrollmentId id, Guid formationId, Guid userId, LearnerProgress progress)
    {
        this.Id = id;
        this.FormationId = formationId;
        this.UserId = userId;
        this.Progress = progress;
    }

    public static FormationEnrollment Create(FormationEnrollmentId id, Guid formationId, Guid userId)
    {
        FormationEnrollment enrollment = new(id, formationId, userId, LearnerProgress.Empty);
        enrollment.Record(new PersonEnrolled(formationId, userId));
        return enrollment;
    }

    public void CompleteModule(Guid moduleId)
    {
        this.Progress = this.Progress.WithModuleCompleted(moduleId, null);
        this.Record(new EnrollmentModuleCompleted(this.FormationId, this.UserId, moduleId, null, this.Progress.CompletedModuleIds.Count));
    }

    public void SubmitExam(Guid moduleId, int score, bool passed, ISystemClock systemClock)
    {
        if (this.Progress.CompletedModuleIds.Contains(moduleId))
        {
            throw FormationEnrollmentErrors.ExamAlreadyPassed(moduleId);
        }

        DateTime attemptedAt = systemClock.UtcNow;
        this.Progress = this.Progress.WithExamAttempt(new ExamAttempt(moduleId, score, passed, attemptedAt));
        this.Record(new EnrollmentExamAttempted(this.FormationId, this.UserId, moduleId, score, passed, attemptedAt));

        if (passed)
        {
            this.Record(new EnrollmentModuleCompleted(this.FormationId, this.UserId, moduleId, score, this.Progress.CompletedModuleIds.Count));
        }
    }

    public void Unenroll() => this.Record(new PersonUnenrolled(this.FormationId, this.UserId));
}
