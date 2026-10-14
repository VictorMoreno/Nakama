using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationEnrollment.Domain;

public sealed class FormationEnrollmentByUserSpecification(Guid userId) : BaseSpecification<FormationEnrollment>
{
    public Guid UserId { get; } = userId;

    protected override Expression<Func<FormationEnrollment, bool>> CreateExpression() =>
        enrollment => enrollment.UserId == this.UserId;
}
