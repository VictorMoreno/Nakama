namespace Nakama.FormationDetail.Domain;

public partial class ChatbotLabelRef
{
    public ChatbotLabelRef(Guid botKey, string label)
    {
        this.Id = botKey;
        this.Label = label;
    }

    public static ChatbotLabelRef Create(Guid botKey, string label) => new(botKey, label);
}
