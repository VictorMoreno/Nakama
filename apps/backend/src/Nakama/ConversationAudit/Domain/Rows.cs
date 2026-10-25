namespace Nakama.ConversationAudit.Domain;

public sealed class AuditMessageRow
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
