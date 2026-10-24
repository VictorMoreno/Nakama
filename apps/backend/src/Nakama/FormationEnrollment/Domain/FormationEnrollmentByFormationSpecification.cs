using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationEnrollment.Domain;

public sealed class FormationEnrollmentByFormationSpecification(Guid formationId) : BaseSpecification<FormationEnrollment>
{
    public Guid FormationId { get; } = formationId;

    protected override Expression<Func<FormationEnrollment, bool>> CreateExpression() =>
        enrollment => enrollment.FormationId == this.FormationId;
}
