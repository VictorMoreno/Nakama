using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;

namespace Nakama.Formations.Domain;

public partial class Formation
{
    private Formation(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status,
        Guid? linkedChatbotKey)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.LinkedChatbotKey = linkedChatbotKey;
    }

    public static Formation Create(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status,
        Guid? linkedChatbotKey)
    {
        var formation = new Formation(key, title, audience, description, status, linkedChatbotKey);
        formation.Record(new FormationCreated(key, title, audience, description, status, linkedChatbotKey));
        return formation;
    }

    public void UpdateStatus(FormationStatus status)
    {
        if (this.Status == status)
        {
            return;
        }

        this.Status = status;
        Record(new FormationStatusChanged(this.Id, status));
    }

    public void UpdateIcon(string? iconUrl)
    {
        this.IconUrl = iconUrl;
        Record(new FormationIconUpdated(this.Id, iconUrl));
    }

    public void AddModule(FormationModule module)
    {
        this._modules.Add(module);
        Record(new FormationModuleAdded(this.Id, module));
    }

    public void UpdateModule(
        ModuleId moduleId,
        string title,
        ModuleType type,
        string summary,
        bool gated,
        bool aiDraft,
        string? resourceUrl,
        int? durationMin,
        IReadOnlyList<QuizQuestion>? quiz,
        int? passingScore)
    {
        var index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        var updated = this._modules[index] with
        {
            Title = title,
            Type = type,
            Summary = summary,
            Gated = gated,
            AiDraft = aiDraft,
            ResourceUrl = resourceUrl,
            DurationMin = durationMin,
            Quiz = quiz,
            PassingScore = passingScore,
        };
        this._modules[index] = updated;

        Record(new FormationModuleUpdated(this.Id, updated));
    }

    public void RemoveModule(ModuleId moduleId)
    {
        var index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        this._modules.RemoveAt(index);

        Record(new FormationModuleRemoved(this.Id, moduleId));
    }

    public void ToggleModuleGating(ModuleId moduleId)
    {
        var index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        var toggled = this._modules[index] with { Gated = !this._modules[index].Gated };
        this._modules[index] = toggled;

        Record(new FormationModuleGatingToggled(this.Id, moduleId, toggled.Gated));
    }

    public void Archive()
    {
        if (this.Archived)
        {
            return;
        }

        this.Archived = true;
        Record(new FormationArchived(this.Id));
    }
}
