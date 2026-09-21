namespace Nakama.ChatbotSummaries.Domain;

public partial class ChatbotSummary
{
    private ChatbotSummary(Guid key, string label, string description, string status, bool general)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
        this.General = general;
    }

    public static ChatbotSummary Create(Guid key, string label, string description, string status, bool general) =>
        new(key, label, description, status, general);

    public void UpdateStatus(string status) => this.Status = status;

    public void MarkArchived() => this.Archived = true;

    public void UpdateIcon(string? iconUrl) => this.IconUrl = iconUrl;

    public void IncrementDocsCount() => this.DocsCount++;

    public void DecrementDocsCount() => this.DocsCount = Math.Max(0, this.DocsCount - 1);

    public void IncrementTeamCount() => this.TeamCount++;

    public void DecrementTeamCount() => this.TeamCount = Math.Max(0, this.TeamCount - 1);
}
