namespace Nakama.ChatbotMembers.Domain;

public partial class ChatbotMember
{
    public ChatbotMember(Guid id, Guid chatbotId, Guid userId)
    {
        this.Id = id;
        this.ChatbotId = chatbotId;
        this.UserId = userId;
    }

    public static ChatbotMember Create(Guid id, Guid chatbotId, Guid userId) => new(id, chatbotId, userId);
}
