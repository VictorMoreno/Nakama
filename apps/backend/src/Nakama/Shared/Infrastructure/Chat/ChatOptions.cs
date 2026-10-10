namespace Nakama.Shared.Infrastructure.Chat;

public sealed class ChatOptions
{
    public const string SectionName = "Chat";

    public required int MaxAttachmentsPerMessage { get; init; }

    public required int MaxAttachmentSizeMb { get; init; }

    public required string[] AllowedAttachmentExtensions { get; init; }
}
