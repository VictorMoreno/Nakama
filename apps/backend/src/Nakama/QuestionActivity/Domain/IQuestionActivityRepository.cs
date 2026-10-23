using Nakama.Shared.Domain.Specification;

namespace Nakama.QuestionActivity.Domain;

public interface IQuestionActivityRepository
{
    Task<IReadOnlyList<QuestionActivityEntry>> Search(ISpecification<QuestionActivityEntry> specification, CancellationToken ct);

    Task Save(QuestionActivityEntry entry, CancellationToken ct);
}
