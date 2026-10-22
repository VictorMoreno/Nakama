using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Chatbots.Infrastructure;

public sealed class NhConversationsRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<Conversation, ConversationId>(unitOfWork), IConversationsRepository
{
    // FindById, Save, Remove inherited directly from the base, no redeclaration needed
}
