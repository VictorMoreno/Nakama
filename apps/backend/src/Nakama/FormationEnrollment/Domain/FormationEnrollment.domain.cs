using Nakama.FormationEnrollment.Domain.Events;

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

    public void CompleteModule(Guid moduleId, int? score)
    {
        this.Progress = this.Progress.WithModuleCompleted(moduleId, score);
        this.Record(new EnrollmentModuleCompleted(this.FormationId, this.UserId, moduleId, score, this.Progress.CompletedModuleIds.Count));
    }

    public void Unenroll() => this.Record(new PersonUnenrolled(this.FormationId, this.UserId));
}
