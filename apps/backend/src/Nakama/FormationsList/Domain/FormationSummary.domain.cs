namespace Nakama.FormationsList.Domain;

public partial class FormationSummary
{
    private FormationSummary(string key, string title, string audience, string description, string status)
    {
        this.Key = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
    }

    public static FormationSummary Create(string key, string title, string audience, string description, string status) =>
        new(key, title, audience, description, status);

    public void UpdateStatus(string status) => this.Status = status;

    /// <summary>Adding a module changes the completion denominator, so re-derive <see cref="ProgressPct"/>.</summary>
    public void IncrementModulesCount()
    {
        this.ModulesCount++;
        Recalculate();
    }

    public void IncrementCompletedModulesCount()
    {
        this.CompletedModulesCount++;
        Recalculate();
    }

    private void Recalculate() => this.ProgressPct = this.ModulesCount == 0 ? 0 : this.CompletedModulesCount * 100 / this.ModulesCount;
}
