namespace Nakama.FormationDetail.Domain;

public partial class FormationDetail
{
    public FormationDetail(
        Guid key,
        string title,
        string audience,
        string description,
        string status,
        bool archived,
        string? iconUrl,
        Guid? linkedChatbotKey,
        string? linkedChatbotLabel,
        List<FormationModuleRow> modules,
        List<FormationMemberRow> team)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.Archived = archived;
        this.IconUrl = iconUrl;
        this.LinkedChatbotKey = linkedChatbotKey;
        this.LinkedChatbotLabel = linkedChatbotLabel;
        this.Modules = modules;
        this.Team = team;
    }

    public static FormationDetail Create(
        Guid key,
        string title,
        string audience,
        string description,
        string status,
        Guid? linkedChatbotKey,
        string? linkedChatbotLabel) =>
        new(
            key,
            title,
            audience,
            description,
            status,
            false,
            null,
            linkedChatbotKey,
            linkedChatbotLabel,
            [],
            []);

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
        FormationModuleRow? module = this.Modules.FirstOrDefault(m => m.Id == moduleId);
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

    public void RemoveModule(Guid moduleId) => this.Modules.RemoveAll(m => m.Id == moduleId);

    public void ToggleModuleGating(Guid moduleId, bool gated)
    {
        FormationModuleRow? module = this.Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module is not null)
        {
            module.Gated = gated;
        }
    }

    public void AddMember(Guid userId)
    {
        if (this.Team.Any(m => m.UserId == userId))
        {
            return;
        }

        this.Team.Add(new FormationMemberRow { UserId = userId, Progress = 0 });
    }

    public void RemoveMember(Guid userId) => this.Team.RemoveAll(m => m.UserId == userId);

    public void UpdateMemberProgress(Guid userId, int completedModuleCount, int totalModuleCount)
    {
        FormationMemberRow? member = this.Team.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            return;
        }

        member.Progress = totalModuleCount == 0 ? 0 : completedModuleCount * 100 / totalModuleCount;
    }
}
