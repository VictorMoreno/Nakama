namespace Nakama.Chatbots.Domain;

public interface IConversationsRepository
{
    Task<Conversation?> FindById(ConversationId id, CancellationToken ct);

    Task Save(Conversation conversation, CancellationToken ct);

    Task Remove(Conversation conversation, CancellationToken ct);
}
