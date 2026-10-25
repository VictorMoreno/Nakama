namespace Nakama.ChatbotDetail.Domain;

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
