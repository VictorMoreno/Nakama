namespace Nakama.ConversationAudit.Domain;

// Embedded (as JSON) inside ConversationAuditLog below - denormalized, immutable record data,
// not a domain value object. Same convention as ChatPage/Domain/Rows.cs.

public sealed class AuditMessageRow
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Citation { get; set; } = string.Empty;
    public string MoreExplanation { get; set; } = string.Empty;
    public string SocraticVariant { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
