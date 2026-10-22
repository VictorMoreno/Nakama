using Nakama.ChatbotMembers.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ChatbotMembers.Infrastructure;

public sealed class NhChatbotMemberRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<ChatbotMember, Guid>(unitOfWork), IChatbotMemberRepository;
