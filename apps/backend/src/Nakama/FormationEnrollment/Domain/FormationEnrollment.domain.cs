using Nakama.FormationEnrollment.Domain.Events;

namespace Nakama.FormationEnrollment.Domain;

public partial class FormationEnrollment
{
    private FormationEnrollment(FormationEnrollmentId id, Guid formationId, Guid userId)
    {
        this.Id = id;
        this.FormationId = formationId;
        this.UserId = userId;
    }

    public static FormationEnrollment Create(FormationEnrollmentId id, Guid formationId, Guid userId)
    {
        var enrollment = new FormationEnrollment(id, formationId, userId);
        enrollment.Record(new PersonEnrolled(formationId, userId));
        return enrollment;
    }

    public void CompleteModule(Guid moduleId, int? score)
    {
        this.Progress = this.Progress.WithModuleCompleted(moduleId, score);
        Record(new EnrollmentModuleCompleted(this.FormationId, this.UserId, moduleId, score, this.Progress.CompletedModuleIds.Count));
    }

    public void Unenroll() => Record(new PersonUnenrolled(this.FormationId, this.UserId));
}
