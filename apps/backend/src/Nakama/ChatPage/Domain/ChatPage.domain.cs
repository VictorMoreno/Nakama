namespace Nakama.ChatPage.Domain;

public partial class ChatPage
{
    private ChatPage(string botKey, string label)
    {
        this.BotKey = botKey;
        this.Label = label;
    }

    public static ChatPage Create(string botKey, string label) => new(botKey, label);

    public void UpdateSettings(bool socratic, bool explicaMas, string provider)
    {
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
    }

    public void StartConversation(string id, string botKey, string title) => this.History.Add(new ConversationSummaryRow
    {
        Id = id,
        BotKey = botKey,
        Title = title,
    });

    public void AppendExchange(string conversationId, string question, string answer, string citation, string moreExplanation, string socraticVariant)
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
