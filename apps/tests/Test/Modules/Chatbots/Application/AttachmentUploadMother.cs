using Nakama.Chatbots.Application.QuestionAsker;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class AttachmentUploadMother
{
    public static ChatbotQuestionAsker.AttachmentUpload Random() => With();

    public static ChatbotQuestionAsker.AttachmentUpload With(string? fileName = null, byte[]? content = null) =>
        new(
            fileName ?? $"{WordMother.Random()}.pdf",
            content ?? BytesMother.Random());
}
