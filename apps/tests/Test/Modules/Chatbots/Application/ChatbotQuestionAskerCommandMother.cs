using Nakama.Chatbots.Application.QuestionAsker;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotQuestionAskerCommandMother
{
    public static ChatbotQuestionAsker.Command Random() => With();

    public static ChatbotQuestionAsker.Command WithAttachmentCount(int count) =>
        With(attachments: Enumerable.Range(0, count).Select(_ => AttachmentUploadMother.Random()).ToList());

    public static ChatbotQuestionAsker.Command With(
        Guid? botKey = null,
        Guid? conversationId = null,
        Guid? userId = null,
        string? title = null,
        string? question = null,
        IReadOnlyList<ChatbotQuestionAsker.AttachmentUpload>? attachments = null) =>
        new(
            botKey ?? Guid.NewGuid(),
            conversationId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            question ?? WordMother.Random(),
            attachments ?? []);
}
