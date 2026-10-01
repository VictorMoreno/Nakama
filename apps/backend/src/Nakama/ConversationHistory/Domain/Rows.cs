namespace Nakama.ConversationHistory.Domain;

// Own copy, not a reference to ChatPage.Domain.ChatExampleRow - same criterion already used
// for ConversationAudit's AuditMessageRow: this bounded context stays textually independent,
// no cross-namespace type dependency, even for a small shared shape.
public sealed class HistoryExampleRow
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Citation { get; set; } = string.Empty;
    public string MoreExplanation { get; set; } = string.Empty;
    public string SocraticVariant { get; set; } = string.Empty;
}

public sealed class ConversationSummaryRow
{
    public Guid Id { get; set; }
    public Guid BotKey { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DateLabel { get; set; }
    public List<HistoryExampleRow> Exchanges { get; set; } = [];
}
