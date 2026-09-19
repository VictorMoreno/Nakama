using Nakama.ChatPage.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatPage.Infrastructure;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed class NhChatPageRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatPage, Guid>(readSessions), IChatPageRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
