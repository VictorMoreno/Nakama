namespace Nakama.ConversationHistory.Domain;

public partial class ConversationHistory
{
    private ConversationHistory(Guid botKey)
    {
        this.Id = botKey;
    }

    public static ConversationHistory Create(Guid botKey) => new(botKey);

    public void StartConversation(Guid id, Guid botKey, Guid userId, string title) => this.Entries.Add(new ConversationSummaryRow
    {
        Id = id,
        BotKey = botKey,
        UserId = userId,
        Title = title,
    });

    public void RemoveConversation(Guid conversationId) => this.Entries.RemoveAll(c => c.Id == conversationId);

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
