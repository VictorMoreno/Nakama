using Nakama.Chatbots.Application.DocumentAdder;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotDocumentAdderCommandMother
{
    public static ChatbotDocumentAdder.Command Random() => With();

    public static ChatbotDocumentAdder.Command With(
        Guid? id = null,
        Guid? documentId = null,
        string? name = null,
        string? origin = null,
        string? originalFileName = null,
        byte[]? content = null)
    {
        byte[] bytes = content ?? MotherCreator.Random.Random.Bytes(16);

        return new ChatbotDocumentAdder.Command(
            id ?? ChatbotMother.RandomId().Value,
            documentId ?? Guid.NewGuid(),
            name ?? WordMother.Random(),
            origin ?? WordMother.Random(),
            originalFileName ?? $"{WordMother.Random()}.pdf",
            new MemoryStream(bytes),
            bytes.Length);
    }
}
