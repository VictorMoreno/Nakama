namespace Nakama.ChatPage.Domain;

public interface IChatPageRepository
{
    Task<ChatPage?> FindById(Guid id, CancellationToken ct);

    Task Save(ChatPage entry, CancellationToken ct);

    Task Update(ChatPage entry, CancellationToken ct);
}
