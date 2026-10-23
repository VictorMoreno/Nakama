namespace Nakama.ConversationHistory.Domain;

public partial class ConversationHistory
{
    public ConversationHistory(Guid botKey, List<ConversationSummaryRow> entries)
    {
        this.Id = botKey;
        this.Entries = entries;
    }

    public static ConversationHistory Create(Guid botKey) => new(botKey, []);

    public void StartConversation(Guid id, Guid botKey, Guid userId, string title, DateTime startedAt) => this.Entries.Add(new ConversationSummaryRow
    {
        Id = id,
        BotKey = botKey,
        UserId = userId,
        Title = title,
        DateLabel = startedAt
    });

    public void RemoveConversation(Guid conversationId) => this.Entries.RemoveAll(c => c.Id == conversationId);

    public void OrderByMostRecent()
    {
        List<ConversationSummaryRow> ordered = this.Entries.OrderByDescending(c => c.DateLabel).ToList();

        this.Entries.Clear();
        this.Entries.AddRange(ordered);
    }

    public void RateExchange(Guid conversationId, int exchangeIndex, bool useful)
    {
        ConversationSummaryRow? conversation = this.Entries.FirstOrDefault(c => c.Id == conversationId);
        if (conversation is not null && exchangeIndex >= 0 && exchangeIndex < conversation.Exchanges.Count)
        {
            conversation.Exchanges[exchangeIndex].Useful = useful;
        }
    }

    public void AppendExchange(Guid conversationId, string question, string answer, string citation, string moreExplanation, string socraticVariant, IEnumerable<string> attachments)
    {
        ConversationSummaryRow? conversation = this.Entries.FirstOrDefault(c => c.Id == conversationId);
        conversation?.Exchanges.Add(new HistoryExampleRow
        {
            Question = question,
            Answer = answer,
            Citation = citation,
            MoreExplanation = moreExplanation,
            SocraticVariant = socraticVariant,
            Attachments = attachments.ToList(),
        });
    }
}
