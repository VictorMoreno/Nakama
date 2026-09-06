using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationDetail.Infrastructure;

public sealed class NhChatbotLabelRefRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotLabelRef, string>(readSessions), IChatbotLabelRefRepository
{
    public Task<ChatbotLabelRef?> FindByBotKey(string botKey, CancellationToken ct) => this.FindById(botKey, ct);
}
