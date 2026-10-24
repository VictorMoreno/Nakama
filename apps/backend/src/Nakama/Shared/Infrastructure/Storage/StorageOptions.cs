namespace Nakama.Shared.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public required string ChatbotIconsPath { get; init; }

    public required string ChatbotIconsRoute { get; init; }

    public required string FormationIconsPath { get; init; }

    public required string FormationIconsRoute { get; init; }

    public required string ChatbotDocumentsPath { get; init; }

    public required string FormationDocumentsPath { get; init; }

    public required string ConversationAttachmentsPath { get; init; }
}
