using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.ChatbotSummaries.Infrastructure;

public sealed class NhChatbotSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotSummary, Guid>(readSessions), IChatbotSummaryRepository
{
    public async Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<ChatbotSummary>().OrderBy(c => c.Label).ToListAsync(ct);
    }
}