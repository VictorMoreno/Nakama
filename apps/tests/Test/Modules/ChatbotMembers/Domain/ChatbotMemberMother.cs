using Nakama.ChatbotMembers.Domain;

namespace Test.Modules.ChatbotMembers.Domain;

public static class ChatbotMemberMother
{
    public static ChatbotMember Random() => With();

    public static ChatbotMember With(Guid? id = null, Guid? chatbotId = null, Guid? userId = null) =>
        new(id ?? Guid.NewGuid(), chatbotId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
}
