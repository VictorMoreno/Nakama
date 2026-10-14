using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationEnrollment.Domain;

public sealed class FormationEnrollmentByFormationAndUserSpecification(Guid formationId, Guid userId)
    : BaseSpecification<FormationEnrollment>
{
    public Guid FormationId { get; } = formationId;

    public Guid UserId { get; } = userId;

    protected override Expression<Func<FormationEnrollment, bool>> CreateExpression() =>
        enrollment => enrollment.FormationId == this.FormationId && enrollment.UserId == this.UserId;
}
