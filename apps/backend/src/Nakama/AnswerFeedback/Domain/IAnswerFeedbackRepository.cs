using Nakama.Shared.Domain.Specification;

namespace Nakama.AnswerFeedback.Domain;

public interface IAnswerFeedbackRepository
{
    Task<AnswerFeedbackEntry?> FindById(Guid id, CancellationToken ct);

    Task<IReadOnlyList<AnswerFeedbackEntry>> Search(ISpecification<AnswerFeedbackEntry> specification, CancellationToken ct);

    Task<int> Count(ISpecification<AnswerFeedbackEntry> specification, CancellationToken ct);

    Task Save(AnswerFeedbackEntry entry, CancellationToken ct);
}
