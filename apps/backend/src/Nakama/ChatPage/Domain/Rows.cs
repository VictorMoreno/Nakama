namespace Nakama.ChatPage.Domain;

// Plain rows embedded (as JSON) inside ChatPage below - denormalized projection data, not
// domain value objects. Live in Domain (not Infrastructure) because they're property types on
// the read entity itself.

public sealed class ChatExampleRow
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
    public string Title { get; set; } = string.Empty;
    public DateTime DateLabel { get; set; }
    public List<ChatExampleRow> Exchanges { get; set; } = [];
}
