namespace Nakama.Chatbots.Domain;

public interface IConversationsRepository
{
    Task<Conversation?> FindByKey(ConversationId id, CancellationToken ct);

    Task Save(Conversation conversation, CancellationToken ct);
}
