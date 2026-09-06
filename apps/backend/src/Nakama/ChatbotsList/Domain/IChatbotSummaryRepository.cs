namespace Nakama.ChatbotsList.Domain;

public interface IChatbotSummaryRepository
{
    Task<ChatbotSummary?> FindByKey(string key, CancellationToken ct);

    Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct);

    Task Save(ChatbotSummary entry, CancellationToken ct);
}
