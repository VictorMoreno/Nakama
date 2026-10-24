using Nakama.ConversationAudit.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationAudit.Domain;

public static class AuditMessageRowMother
{
    public static AuditMessageRow Random() => With();

    public static AuditMessageRow With(DateTime? recordedAt = null) =>
        new()
        {
            Question = WordMother.Random(),
            Answer = WordMother.Random(),
            RecordedAt = recordedAt ?? DateTime.UtcNow
        };
}
