using Nakama.ConversationAudit.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.ConversationAudit.Infrastructure;

public sealed class NhConversationAuditRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<ConversationAuditLog, Guid>(unitOfWork), IConversationAuditRepository
{
    // FindById, Save, Update inherited directly from the base, no redeclaration needed
    // (Update is Save under the hood, same as every other read-model repository).
}
