using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ConversationHistory.Infrastructure;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class NhConversationHistoryRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<ConversationHistory, Guid>(unitOfWork), IConversationHistoryRepository
{
    public new async Task<ConversationHistory?> FindById(Guid botKey, CancellationToken ct)
    {
        ConversationHistory? history = await base.FindById(botKey, ct);
        history?.OrderByMostRecent();
        return history;
    }
}
