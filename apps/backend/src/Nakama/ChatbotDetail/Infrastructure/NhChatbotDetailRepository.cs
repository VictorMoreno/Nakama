using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotDetail.Infrastructure;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class NhChatbotDetailRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotDetail, string>(readSessions), IChatbotDetailRepository
{
    public Task<ChatbotDetail?> FindByKey(string key, CancellationToken ct) => this.FindById(key, ct);
}
