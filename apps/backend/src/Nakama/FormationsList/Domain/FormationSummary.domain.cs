namespace Nakama.FormationsList.Domain;

public partial class FormationSummary
{
    public FormationSummary(
        Guid key,
        string title,
        string audience,
        string description,
        string status,
        bool archived,
        string? iconUrl,
        int modulesCount)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.Archived = archived;
        this.IconUrl = iconUrl;
        this.ModulesCount = modulesCount;
    }

    public static FormationSummary Create(Guid key, string title, string audience, string description, string status) =>
        new(key, title, audience, description, status, false, null, 0);

    public void UpdateStatus(string status) => this.Status = status;

    public void MarkArchived() => this.Archived = true;

    public void MarkUnarchived() => this.Archived = false;

    public void UpdateIcon(string? iconUrl) => this.IconUrl = iconUrl;

    public void IncrementModulesCount() => this.ModulesCount++;

    public void DecrementModulesCount() => this.ModulesCount = Math.Max(0, this.ModulesCount - 1);
}
