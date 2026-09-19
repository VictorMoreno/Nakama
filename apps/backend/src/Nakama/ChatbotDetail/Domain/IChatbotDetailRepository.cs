namespace Nakama.ChatbotDetail.Domain;

public interface IChatbotDetailRepository
{
    Task<ChatbotDetail?> FindById(Guid id, CancellationToken ct);

    Task Save(ChatbotDetail entry, CancellationToken ct);

    Task Update(ChatbotDetail entry, CancellationToken ct);
}
