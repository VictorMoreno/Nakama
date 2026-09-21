namespace Nakama.ChatPage.Domain;

public partial class ChatPage
{
    private ChatPage(Guid botKey, string label, bool socratic, bool explicaMas, string provider)
    {
        this.Id = botKey;
        this.Label = label;
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
    }

    public static ChatPage Create(Guid botKey, string label, bool socratic, bool explicaMas, string provider) =>
        new(botKey, label, socratic, explicaMas, provider);

    public void UpdateSettings(bool socratic, bool explicaMas, string provider)
    {
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
    }

    public void StartConversation(Guid id, Guid botKey, string title) => this.History.Add(new ConversationSummaryRow
    {
        Id = id,
        BotKey = botKey,
        Title = title,
    });

    public void AppendExchange(Guid conversationId, string question, string answer, string citation, string moreExplanation, string socraticVariant)
    {
        var conversation = this.History.FirstOrDefault(c => c.Id == conversationId);
        conversation?.Exchanges.Add(new ChatExampleRow
        {
            Question = question,
            Answer = answer,
            Citation = citation,
            MoreExplanation = moreExplanation,
            SocraticVariant = socraticVariant,
        });
    }
}
