using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;

namespace Nakama.Formations.Domain;

public partial class Formation
{
    public const int MinPassingScore = 1;
    public const int MaxPassingScore = 100;

    public Formation(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status,
        string? iconUrl,
        bool archived,
        IReadOnlyList<FormationModule> modules,
        IReadOnlyList<FormationDocument> documents)
    {
        this.Id = key;
        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Status = status;
        this.IconUrl = iconUrl;
        this.Archived = archived;
        this._modules.AddRange(modules);
        this._documents.AddRange(documents);
    }

    public static Formation Create(
        FormationId key,
        string title,
        string audience,
        string description,
        FormationStatus status)
    {
        Formation formation = new(key, title, audience, description, status, null, false, [], []);
        formation.Record(new FormationCreated(key, title, audience, description, status));
        return formation;
    }

    public void UpdateStatus(FormationStatus status)
    {
        this.EnsureNotArchived();

        if (this.Status == status)
        {
            return;
        }

        this.Status = status;
        this.Record(new FormationStatusChanged(this.Id, status));
    }

    public void UpdateDetails(string title, string audience, string description)
    {
        this.EnsureNotArchived();

        if (this.Title == title && this.Audience == audience && this.Description == description)
        {
            return;
        }

        this.Title = title;
        this.Audience = audience;
        this.Description = description;
        this.Record(new FormationDetailsUpdated(this.Id, title, audience, description));
    }

    public void UpdateIcon(string? iconUrl)
    {
        this.EnsureNotArchived();

        this.IconUrl = iconUrl;
        this.Record(new FormationIconUpdated(this.Id, iconUrl));
    }

    public void AddModule(FormationModule module)
    {
        this.EnsureNotArchived();
        EnsureValidExamData(module.Type, module.Quiz, module.PassingScore);

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
        this.EnsureNotArchived();
        EnsureValidExamData(type, quiz, passingScore);

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
        this.EnsureNotArchived();

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
        this.EnsureNotArchived();

        int index = this._modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.Value.ToString(), this.Id.Value.ToString());
        }

        FormationModule toggled = this._modules[index] with { Gated = !this._modules[index].Gated };
        this._modules[index] = toggled;

        this.Record(new FormationModuleGatingToggled(this.Id, moduleId, toggled.Gated));
    }

    public void AddDocument(FormationDocument document)
    {
        this.EnsureNotArchived();

        this._documents.Add(document);
        this.Record(new FormationDocumentAdded(this.Id, document));
    }

    public void RemoveDocument(Guid documentId)
    {
        this.EnsureNotArchived();

        if (this._documents.All(existing => existing.Id != documentId))
        {
            throw FormationErrors.DocumentNotFound(documentId);
        }

        this._documents.RemoveAll(existing => existing.Id == documentId);
        this.Record(new FormationDocumentRemoved(this.Id, documentId));
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

    public void Unarchive()
    {
        if (!this.Archived)
        {
            return;
        }

        this.Archived = false;
        this.Record(new FormationUnarchived(this.Id));
    }

    private static void EnsureValidExamData(ModuleType type, IReadOnlyList<QuizQuestion>? quiz, int? passingScore)
    {
        if (type != ModuleType.Exam)
        {
            if (quiz is not null || passingScore is not null)
            {
                throw FormationErrors.ExamDataOnNonExamModule;
            }

            return;
        }

        if (quiz is null || quiz.Count == 0)
        {
            throw FormationErrors.ExamQuizRequired;
        }

        if (passingScore is null || passingScore < MinPassingScore || passingScore > MaxPassingScore)
        {
            throw FormationErrors.ExamPassingScoreInvalid(MinPassingScore, MaxPassingScore);
        }
    }

    private void EnsureNotArchived()
    {
        if (this.Archived)
        {
            throw FormationErrors.Archived(this.Id);
        }
    }
}
