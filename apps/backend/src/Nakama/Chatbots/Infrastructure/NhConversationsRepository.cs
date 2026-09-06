using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Chatbots.Infrastructure;

public sealed class NhConversationsRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<Conversation, ConversationId>(writeSessions), IConversationsRepository
{
    public Task<Conversation?> FindByKey(ConversationId id, CancellationToken ct) => this.FindById(id, ct);
}
