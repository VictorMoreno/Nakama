using Nakama.Shared.Domain;

namespace Nakama.ChatbotSummaries.Domain;

public partial class ChatbotSummary : IEntity
{
    protected ChatbotSummary() { }

    public Guid Id { get; protected set; }
    public string Label { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = string.Empty;
    public int DocsCount { get; protected set; }
    public int TeamCount { get; protected set; }
    public bool Archived { get; protected set; }
    public string? IconUrl { get; protected set; }
}
