using Nakama.ConversationAudit.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationAudit.Domain;

public static class ConversationAuditLogMother
{
    public static ConversationAuditLog Create(
        Guid id,
        Guid userId,
        Guid botKey,
        string title,
        DateTime startedAt,
        List<AuditMessageRow> messages) =>
        new(id, userId, botKey, title, startedAt, messages);

    public static ConversationAuditLog Random() => With();

    public static ConversationAuditLog With(
        Guid? id = null,
        Guid? userId = null,
        Guid? botKey = null,
        string? title = null,
        DateTime? startedAt = null,
        List<AuditMessageRow>? messages = null) =>
        Create(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            botKey ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            startedAt ?? DateTime.UtcNow,
            messages ?? []);
}
