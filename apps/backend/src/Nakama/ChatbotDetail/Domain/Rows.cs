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
    public DateTime Updated { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public sealed class TeamMemberRow
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
