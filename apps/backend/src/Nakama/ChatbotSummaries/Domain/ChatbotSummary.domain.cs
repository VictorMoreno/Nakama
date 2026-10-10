namespace Nakama.ChatbotSummaries.Domain;

public partial class ChatbotSummary
{
    private ChatbotSummary(Guid key, string label, string description, string status)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
    }

    public static ChatbotSummary Create(Guid key, string label, string description, string status) =>
        new(key, label, description, status);

    public void UpdateStatus(string status) => this.Status = status;

    public void MarkArchived() => this.Archived = true;

    public void UpdateIcon(string? iconUrl) => this.IconUrl = iconUrl;

    public void IncrementDocsCount() => this.DocsCount++;

    public void DecrementDocsCount() => this.DocsCount = Math.Max(0, this.DocsCount - 1);

    public void IncrementTeamCount() => this.TeamCount++;

    public void DecrementTeamCount() => this.TeamCount = Math.Max(0, this.TeamCount - 1);
}
