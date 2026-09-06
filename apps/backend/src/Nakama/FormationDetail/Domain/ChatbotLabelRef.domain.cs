namespace Nakama.FormationDetail.Domain;

public partial class ChatbotLabelRef
{
    private ChatbotLabelRef(string botKey, string label)
    {
        this.BotKey = botKey;
        this.Label = label;
    }

    public static ChatbotLabelRef Create(string botKey, string label) => new(botKey, label);
}
