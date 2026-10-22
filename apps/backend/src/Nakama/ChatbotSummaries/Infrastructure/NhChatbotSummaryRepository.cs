using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotSummaries.Infrastructure;

public sealed class NhChatbotSummaryRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<ChatbotSummary, Guid>(unitOfWork), IChatbotSummaryRepository;
