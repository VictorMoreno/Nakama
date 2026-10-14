using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationsList.Domain;

public interface IFormationSummaryRepository
{
    Task<FormationSummary?> FindById(Guid id, CancellationToken ct);

    Task<IReadOnlyList<FormationSummary>> Search(ISpecification<FormationSummary> specification, CancellationToken ct);

    Task Save(FormationSummary entry, CancellationToken ct);

    Task Update(FormationSummary entry, CancellationToken ct);
}
