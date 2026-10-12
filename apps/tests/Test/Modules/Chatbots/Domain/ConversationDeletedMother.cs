using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationDeletedMother
{
    public static ConversationDeleted Random() => With();

    public static ConversationDeleted With(
        ConversationId? id = null,
        ChatbotId? botKey = null,
        IReadOnlyList<string>? attachmentFileNames = null) =>
        new(
            id ?? ConversationMother.RandomId(),
            botKey ?? ChatbotMother.RandomId(),
            attachmentFileNames ?? []);
}
