namespace Nakama.FormationDetail.Domain;

public partial class FormationDetail
{
    private FormationDetail(
        Guid key,
        string title,
        string audience,
        string description,
        string status,
        Guid? linkedChatbotKey,
        string? linkedChatbotLabel)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.LinkedChatbotKey = linkedChatbotKey;
        this.LinkedChatbotLabel = linkedChatbotLabel;
    }

    public static FormationDetail Create(
        Guid key,
        string title,
        string audience,
        string description,
        string status,
        Guid? linkedChatbotKey,
        string? linkedChatbotLabel) =>
        new(key, title, audience, description, status, linkedChatbotKey, linkedChatbotLabel);

    public void UpdateStatus(string status) => this.Status = status;

    public void MarkArchived() => this.Archived = true;

    public void UpdateIcon(string? iconUrl) => this.IconUrl = iconUrl;

    public void AddModule(
        Guid id,
        string title,
        string type,
        string summary,
        bool gated,
        bool aiDraft,
        string? resourceUrl,
        int? durationMin,
        int? passingScore,
        List<QuizQuestionRow>? quiz) => this.Modules.Add(new FormationModuleRow
    {
        Id = id,
        Title = title,
        Type = type,
        Summary = summary,
        Gated = gated,
        AiDraft = aiDraft,
        ResourceUrl = resourceUrl,
        DurationMin = durationMin,
        PassingScore = passingScore,
        Quiz = quiz,
    });

    public void UpdateModule(
        Guid moduleId,
        string title,
        string type,
        string summary,
        bool gated,
        bool aiDraft,
        string? resourceUrl,
        int? durationMin,
        int? passingScore,
        List<QuizQuestionRow>? quiz)
    {
        var module = this.Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module is null)
        {
            return;
        }

        module.Title = title;
        module.Type = type;
        module.Summary = summary;
        module.Gated = gated;
        module.AiDraft = aiDraft;
        module.ResourceUrl = resourceUrl;
        module.DurationMin = durationMin;
        module.PassingScore = passingScore;
        module.Quiz = quiz;
    }

    public void ToggleModuleGating(Guid moduleId, bool gated)
    {
        var module = this.Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module is not null)
        {
            module.Gated = gated;
        }
    }

    public void CompleteModule(Guid moduleId, int? score)
    {
        if (!this.CompletedModuleIds.Contains(moduleId))
        {
            this.CompletedModuleIds.Add(moduleId);
        }

        if (score is not null)
        {
            this.ExamScores[moduleId] = score.Value;
        }
    }

    public void AddTeamMember(Guid userId) => this.Team.Add(new FormationMemberRow
    {
        UserId = userId,
        Progress = 0,
    });

    public void RemoveTeamMember(int index)
    {
        if (index >= 0 && index < this.Team.Count)
        {
            this.Team.RemoveAt(index);
        }
    }
}
