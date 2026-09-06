using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Chatbot
{
    private Chatbot(
        ChatbotKey key,
        string label,
        string description,
        ChatbotStatus status,
        bool general,
        ChatbotSettings settings,
        DateTime lastUsed)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
        this.General = general;
        this.Settings = settings;
        this.LastUsed = lastUsed;
    }

    public static Chatbot Create(
        ChatbotKey key,
        string label,
        string description,
        ChatbotStatus status,
        bool general,
        ChatbotSettings settings,
        ISystemClock systemClock)
    {
        var chatbot = new Chatbot(key, label, description, status, general, settings, systemClock.UtcNow);
        chatbot.Raise(new ChatbotCreated(key, label, description, status, general));
        return chatbot;
    }

    public void UpdateStatus(ChatbotStatus status)
    {
        if (this.Status == status)
        {
            return;
        }

        this.Status = status;
        Raise(new ChatbotStatusChanged(this.Key, status));
    }

    public void UpdateSettings(ChatbotSettings settings)
    {
        this.Settings = settings;
        Raise(new ChatbotSettingsUpdated(this.Key, settings));
    }

    public void UpdateIcon(string? iconUrl)
    {
        this.IconUrl = iconUrl;
        Raise(new ChatbotIconUpdated(this.Key, iconUrl));
    }

    public void AddDocument(ChatbotDocument document)
    {
        this._documents.Add(document);
        Raise(new ChatbotDocumentAdded(this.Key, document));
    }

    public void RemoveDocument(int index)
    {
        this._documents.RemoveAt(index);
        Raise(new ChatbotDocumentRemoved(this.Key, index));
    }

    public void AddTeamMember(TeamMember member)
    {
        this._team.Add(member);
        Raise(new ChatbotTeamMemberAdded(this.Key, member));
    }

    /// <summary>Throws <see cref="ChatbotErrors.OwnerCannotBeRemoved"/> instead of silently allowing the owner to be removed.</summary>
    public void RemoveTeamMember(int index)
    {
        if (this._team[index].IsOwner)
        {
            throw ChatbotErrors.OwnerCannotBeRemoved;
        }

        this._team.RemoveAt(index);
        Raise(new ChatbotTeamMemberRemoved(this.Key, index));
    }

    public void Archive()
    {
        if (this.Archived)
        {
            return;
        }

        this.Archived = true;
        Raise(new ChatbotArchived(this.Key));
    }
}
