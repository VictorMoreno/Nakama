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
        this.Settings = settings;
        this.Record(new ChatbotSettingsUpdated(this.Id, settings));
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

    public void RemoveDocument(int index)
    {
        this._documents.RemoveAt(index);
        this.Record(new ChatbotDocumentRemoved(this.Id, index));
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

    public void RemoveTeamMember(int index)
    {
        if (this._team[index].IsOwner)
        {
            throw ChatbotErrors.OwnerCannotBeRemoved;
        }

        this._team.RemoveAt(index);
        this.Record(new ChatbotTeamMemberRemoved(this.Id, index));
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
