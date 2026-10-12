using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain;

public partial class FormationEnrollment : AggregateRoot<FormationEnrollmentId>
{
    protected FormationEnrollment()
    {
    }

    public Guid FormationId { get; private set; }

    public Guid UserId { get; private set; }

    public LearnerProgress Progress { get; private set; } = LearnerProgress.Empty;
}
