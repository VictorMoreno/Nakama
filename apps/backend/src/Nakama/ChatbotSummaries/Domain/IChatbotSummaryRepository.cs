namespace Nakama.ChatbotSummaries.Domain;

public interface IChatbotSummaryRepository
{
    Task<ChatbotSummary?> FindById(Guid id, CancellationToken ct);

    Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct);

    Task Save(ChatbotSummary entry, CancellationToken ct);

    Task Update(ChatbotSummary entry, CancellationToken ct);
}
