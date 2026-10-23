using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;

namespace Nakama.Chatbots.Domain;

public partial class Chatbot
{
    public Chatbot(
        ChatbotId key,
        string label,
        string description,
        ChatbotStatus status,
        ChatbotSettings settings,
        string? iconUrl,
        bool archived,
        IReadOnlyList<TeamMember> team,
        IReadOnlyList<ChatbotDocument> documents)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
        this.Settings = settings;
        this.IconUrl = iconUrl;
        this.Archived = archived;
        this._team.AddRange(team);
        this._documents.AddRange(documents);
    }

    public static Chatbot Create(
        ChatbotId key,
        string label,
        string description,
        ChatbotStatus status,
        ChatbotSettings settings)
    {
        EnsureHasModel(settings);

        Chatbot chatbot = new(key, label, description, status, settings, null, false, [], []);
        chatbot.Record(new ChatbotCreated(key, label, description, status, settings));
        return chatbot;
    }

    public void UpdateStatus(ChatbotStatus status)
    {
        if (this.Status == status)
        {
            return;
        }

        this.Status = status;
        this.Record(new ChatbotStatusChanged(this.Id, status));
    }

    public void UpdateSettings(ChatbotSettings settings)
    {
        EnsureHasModel(settings);

        this.Settings = settings;
        this.Record(new ChatbotSettingsUpdated(this.Id, settings));
    }

    private static void EnsureHasModel(ChatbotSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            throw ChatbotErrors.ModelRequired;
        }
    }

    public void UpdateIcon(string? iconUrl)
    {
        this.IconUrl = iconUrl;
        this.Record(new ChatbotIconUpdated(this.Id, iconUrl));
    }

    public void AddDocument(ChatbotDocument document)
    {
        this._documents.Add(document);
        this.Record(new ChatbotDocumentAdded(this.Id, document));
    }

    public void RemoveDocument(Guid documentId)
    {
        if (!this._documents.Any(existing => existing.Id == documentId))
        {
            throw ChatbotErrors.DocumentNotFound(documentId);
        }

        this._documents.RemoveAll(existing => existing.Id == documentId);
        this.Record(new ChatbotDocumentRemoved(this.Id, documentId));
    }

    public void AddTeamMember(TeamMember member)
    {
        if (this._team.Any(existing => existing.UserId == member.UserId))
        {
            throw ChatbotErrors.TeamMemberAlreadyExists(member.UserId);
        }

        this._team.Add(member);
        this.Record(new ChatbotTeamMemberAdded(this.Id, member));
    }

    public void RemoveTeamMember(Guid userId)
    {
        TeamMember member = this._team.FirstOrDefault(existing => existing.UserId == userId)
                            ?? throw ChatbotErrors.TeamMemberNotFound(userId);

        if (member.IsOwner)
        {
            throw ChatbotErrors.OwnerCannotBeRemoved;
        }

        this._team.Remove(member);
        this.Record(new ChatbotTeamMemberRemoved(this.Id, userId));
    }

    public void Archive()
    {
        if (this.Archived)
        {
            return;
        }

        this.Archived = true;
        this.Record(new ChatbotArchived(this.Id));
    }
}
