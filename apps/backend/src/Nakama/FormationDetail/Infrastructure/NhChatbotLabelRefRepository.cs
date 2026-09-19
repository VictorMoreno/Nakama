using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationDetail.Infrastructure;

public sealed class NhChatbotLabelRefRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<ChatbotLabelRef, Guid>(readSessions), IChatbotLabelRefRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
