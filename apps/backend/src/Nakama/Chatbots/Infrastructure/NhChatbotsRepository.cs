using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Chatbots.Infrastructure;

public sealed class NhChatbotsRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<Chatbot, ChatbotId>(unitOfWork), IChatbotsRepository;
