using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotsList.Infrastructure;

public sealed class NhChatbotSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotSummary, Guid>(readSessions), IChatbotSummaryRepository
{
    public Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);

    // FindById, Save inherited directly from the base, no redeclaration needed
}
