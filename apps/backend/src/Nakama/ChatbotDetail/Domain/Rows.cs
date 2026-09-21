namespace Nakama.ChatbotDetail.Domain;

// Plain rows embedded (as JSON, see Shared/Infrastructure/Persistence/Nh/UserTypes/JsonUserType)
// inside ChatbotDetail below. Kept as plain POCOs rather than domain value objects - these
// are denormalized projection data, not behavior-bearing domain types, and live in Domain (not
// Infrastructure) only because they're property types on the read entity itself.

public sealed class ChatbotDocumentRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime Updated { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public sealed class TeamMemberRow
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class GapQuestionRow
{
    public string Question { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class StatTileRow
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Bar { get; set; }
}
