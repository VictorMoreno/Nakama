using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotDetail.Infrastructure;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class NhChatbotDetailRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<ChatbotDetail, Guid>(unitOfWork), IChatbotDetailRepository;
