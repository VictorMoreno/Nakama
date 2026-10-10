namespace Nakama.Shared.Infrastructure.Icons;

public sealed class IconsOptions
{
    public const string SectionName = "Icons";

    public required int MaxSizeMb { get; init; }

    public required Dictionary<string, string> ContentTypeExtensions { get; init; }
}
