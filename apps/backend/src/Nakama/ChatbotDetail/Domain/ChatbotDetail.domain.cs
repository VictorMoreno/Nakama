namespace Nakama.ChatbotDetail.Domain;

public partial class ChatbotDetail
{
    public ChatbotDetail(
        Guid key,
        string label,
        string description,
        string status,
        string context,
        bool archived,
        string? iconUrl,
        bool socratic,
        bool explicaMas,
        string provider,
        string model,
        List<ChatbotDocumentRow> docs,
        List<TeamMemberRow> team)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
        this.Context = context;
        this.Archived = archived;
        this.IconUrl = iconUrl;
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
        this.Model = model;
        this.Docs = docs;
        this.Team = team;
    }

    public static ChatbotDetail Create(Guid key,
        string label,
        string description,
        string status,
        bool socratic,
        bool explicaMas,
        string provider,
        string model) =>
        new(
            key,
            label,
            description,
            status,
            string.Empty,
            false,
            null,
            socratic,
            explicaMas,
            provider,
            model,
            [],
            []);

    public void UpdateStatus(string status) => this.Status = status;

    public void UpdateSettings(bool socratic, bool explicaMas, string provider, string model)
    {
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
        this.Model = model;
    }

    public void UpdateIcon(string? iconUrl) => this.IconUrl = iconUrl;

    public void MarkArchived() => this.Archived = true;

    public void AddDocument(Guid id, string name, string origin, DateTime updated, string fileName) =>
        this.Docs.Add(new ChatbotDocumentRow
        {
            Id = id,
            Name = name,
            Origin = origin,
            Updated = updated,
            FileName = fileName,
        });

    public void RemoveDocument(Guid documentId) => this.Docs.RemoveAll(document => document.Id == documentId);

    public void AddTeamMember(Guid userId, string role) => this.Team.Add(new TeamMemberRow
    {
        UserId = userId,
        Role = role,
    });

    public void RemoveTeamMember(Guid userId) => this.Team.RemoveAll(member => member.UserId == userId);
}