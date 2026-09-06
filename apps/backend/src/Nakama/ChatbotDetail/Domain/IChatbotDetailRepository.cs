namespace Nakama.ChatbotDetail.Domain;

public interface IChatbotDetailRepository
{
    Task<ChatbotDetail?> FindByKey(string key, CancellationToken ct);

    Task Save(ChatbotDetail entry, CancellationToken ct);
}
