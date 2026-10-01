namespace Nakama.ConversationAudit.Domain;

public interface IConversationAuditRepository
{
    Task<ConversationAuditLog?> FindById(Guid id, CancellationToken ct);

    Task Save(ConversationAuditLog log, CancellationToken ct);

    Task Update(ConversationAuditLog log, CancellationToken ct);

    // No Remove/Delete on purpose - nothing in the application layer should ever be able to
    // erase an audit entry. The live Conversation/ChatPage.History rows are what the user's
    // delete action touches; this log survives that untouched.
}
