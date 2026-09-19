using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Domain;

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
        formation.Raise(new FormationCreated(key, title, audience, description, status, linkedChatbotKey));
        return formation;
    }

    public void UpdateStatus(FormationStatus status)
    {
        if (this.Status == status)
        {
            return;
        }

        this.Status = status;
        Raise(new FormationStatusChanged(this.Id, status));
    }

    public void AddModule(FormationModule module)
    {
        this._modules.Add(module);
        Raise(new FormationModuleAdded(this.Id, module));
    }

    public void ToggleModuleGating(ModuleId moduleId)
    {
        var index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value, this.Id.Value.ToString());
        }

        var toggled = this._modules[index] with { Gated = !this._modules[index].Gated };
        this._modules[index] = toggled;

        Raise(new FormationModuleGatingToggled(this.Id, moduleId, toggled.Gated));
    }

    public void CompleteModule(ModuleId moduleId, int? score)
    {
        var index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value, this.Id.Value.ToString());
        }

        if (index > 0)
        {
            var previous = this._modules[index - 1];
            if (previous.Gated && !this.MyProgress.CompletedModuleIds.Contains(previous.Id.Value))
            {
                throw FormationErrors.ModuleLockedByGating;
            }
        }

        this.MyProgress = this.MyProgress.WithModuleCompleted(moduleId.Value, score);
        Raise(new FormationModuleCompleted(this.Id, moduleId, score));
    }
}
