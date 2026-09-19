using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Chatbots.Infrastructure;

public sealed class NhChatbotsRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<Chatbot, ChatbotId>(writeSessions), IChatbotsRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
