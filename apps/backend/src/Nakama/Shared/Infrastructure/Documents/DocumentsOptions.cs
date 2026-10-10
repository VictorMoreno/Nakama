namespace Nakama.Shared.Infrastructure.Documents;

public sealed class DocumentsOptions
{
    public const string SectionName = "Documents";

    public required int MaxSizeMb { get; init; }

    public required string[] AllowedExtensions { get; init; }
}
