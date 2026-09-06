using Nakama.ChatPage.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatPage.Infrastructure;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed class NhChatPageRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatPage, string>(readSessions), IChatPageRepository
{
    public Task<ChatPage?> FindByBotKey(string botKey, CancellationToken ct) => this.FindById(botKey, ct);
}
