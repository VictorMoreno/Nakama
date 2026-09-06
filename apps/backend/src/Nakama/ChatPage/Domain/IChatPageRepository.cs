namespace Nakama.ChatPage.Domain;

public interface IChatPageRepository
{
    Task<ChatPage?> FindByBotKey(string botKey, CancellationToken ct);

    Task Save(ChatPage entry, CancellationToken ct);
}
