using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotSummaries.Infrastructure;

public sealed class NhChatbotSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotSummary, Guid>(readSessions), IChatbotSummaryRepository;
