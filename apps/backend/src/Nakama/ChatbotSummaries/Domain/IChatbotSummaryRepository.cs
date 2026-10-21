using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotSummaries.Domain;

public interface IChatbotSummaryRepository
{
    Task<ChatbotSummary?> FindById(Guid id, CancellationToken ct);

    Task<IReadOnlyList<ChatbotSummary>> Search(ISpecification<ChatbotSummary> specification, CancellationToken ct);

    Task<PagedResult<ChatbotSummary>> SearchPage(ISpecification<ChatbotSummary> specification, CancellationToken ct);

    Task Save(ChatbotSummary entry, CancellationToken ct);

    Task Update(ChatbotSummary entry, CancellationToken ct);
}
