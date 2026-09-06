using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Chatbots.Infrastructure;

public sealed class NhChatbotsRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<Chatbot, ChatbotKey>(writeSessions), IChatbotsRepository
{
    public Task<Chatbot?> FindByKey(ChatbotKey key, CancellationToken ct) => this.FindById(key, ct);
}
