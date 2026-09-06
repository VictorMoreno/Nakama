using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotsList.Infrastructure;

public sealed class NhChatbotSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotSummary, string>(readSessions), IChatbotSummaryRepository
{
    public Task<ChatbotSummary?> FindByKey(string key, CancellationToken ct) => this.FindById(key, ct);

    public Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);
}
