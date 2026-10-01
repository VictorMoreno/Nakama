namespace Nakama.ConversationHistory.Domain;

public interface IConversationHistoryRepository
{
    Task<ConversationHistory?> FindById(Guid botKey, CancellationToken ct);

    Task Save(ConversationHistory history, CancellationToken ct);

    Task Update(ConversationHistory history, CancellationToken ct);
}
