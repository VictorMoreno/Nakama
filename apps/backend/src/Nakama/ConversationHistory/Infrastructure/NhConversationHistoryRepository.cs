using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ConversationHistory.Infrastructure;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class NhConversationHistoryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ConversationHistory, Guid>(readSessions), IConversationHistoryRepository
{
    // FindById, Save, Update inherited directly from the base, no redeclaration needed
    // (Update is Save under the hood, same as every other read-model repository).
}
