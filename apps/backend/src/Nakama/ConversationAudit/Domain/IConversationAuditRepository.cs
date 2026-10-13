namespace Nakama.ConversationAudit.Domain;

public interface IConversationAuditRepository
{
    Task<ConversationAuditLog?> FindById(Guid id, CancellationToken ct);
    Task Save(ConversationAuditLog log, CancellationToken ct);
    Task Update(ConversationAuditLog log, CancellationToken ct);
}
