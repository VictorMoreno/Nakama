using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;

namespace Nakama.Formations.Domain;

public partial class Formation
{
    public Formation(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status,
        Guid? linkedChatbotKey,
        string? iconUrl,
        bool archived,
        IReadOnlyList<FormationModule> modules)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.LinkedChatbotKey = linkedChatbotKey;
        this.IconUrl = iconUrl;
        this.Archived = archived;
        this._modules.AddRange(modules);
    }

    public static Formation Create(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status,
        Guid? linkedChatbotKey)
    {
        Formation formation = new(key, title, audience, description, status, linkedChatbotKey, null, false, []);
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
        this.Record(new FormationStatusChanged(this.Id, status));
    }

    public void UpdateIcon(string? iconUrl)
    {
        this.IconUrl = iconUrl;
        this.Record(new FormationIconUpdated(this.Id, iconUrl));
    }

    public void AddModule(FormationModule module)
    {
        this._modules.Add(module);
        this.Record(new FormationModuleAdded(this.Id, module));
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
        int index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        FormationModule updated = this._modules[index] with
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

        this.Record(new FormationModuleUpdated(this.Id, updated));
    }

    public void RemoveModule(ModuleId moduleId)
    {
        int index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        this._modules.RemoveAt(index);

        this.Record(new FormationModuleRemoved(this.Id, moduleId));
    }

    public void ToggleModuleGating(ModuleId moduleId)
    {
        int index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        FormationModule toggled = this._modules[index] with { Gated = !this._modules[index].Gated };
        this._modules[index] = toggled;

        this.Record(new FormationModuleGatingToggled(this.Id, moduleId, toggled.Gated));
    }

    public void Archive()
    {
        if (this.Archived)
        {
            return;
        }

        this.Archived = true;
        this.Record(new FormationArchived(this.Id));
    }
}
