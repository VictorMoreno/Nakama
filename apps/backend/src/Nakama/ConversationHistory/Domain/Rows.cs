namespace Nakama.ConversationHistory.Domain;

public sealed class HistoryExampleRow
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Citation { get; set; } = string.Empty;
    public string MoreExplanation { get; set; } = string.Empty;
    public string SocraticVariant { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = [];
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
