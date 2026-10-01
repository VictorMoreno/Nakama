using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain;

// A person's assignment to + progress in a formation, fused into one aggregate: being enrolled
// IS having a (possibly empty) progress record, there's no "team roster" separate from
// "who's actually progressing" - removing an enrollment removes that person's progress too.
public partial class FormationEnrollment : AggregateRoot<FormationEnrollmentId>
{
    protected FormationEnrollment()
    {
    }

    public virtual Guid FormationId { get; private set; }

    public virtual Guid UserId { get; private set; }

    public virtual LearnerProgress Progress { get; private set; } = LearnerProgress.Empty;
}
