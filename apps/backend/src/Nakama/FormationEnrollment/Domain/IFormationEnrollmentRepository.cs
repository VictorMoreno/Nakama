using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationEnrollment.Domain;

public interface IFormationEnrollmentRepository
{
    Task<IReadOnlyList<FormationEnrollment>> Search(ISpecification<FormationEnrollment> specification, CancellationToken ct);

    Task Save(FormationEnrollment enrollment, CancellationToken ct);

    Task Remove(FormationEnrollment enrollment, CancellationToken ct);
}
