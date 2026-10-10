namespace Nakama.ChatbotDetail.Domain;

public partial class ChatbotDetail
{
    private ChatbotDetail(Guid key,
        string label,
        string description,
        string status,
        bool socratic,
        bool explicaMas,
        string provider,
        string model)
    {
        this.Id = key;
        this.Label = label;
        this.Description = description;
        this.Status = status;
        this.Socratic = socratic;
        this.ExplicaMas = explicaMas;
        this.Provider = provider;
        this.Model = model;
    }

    public static ChatbotDetail Create(Guid key,
        string label,
        string description,
        string status,
        bool socratic,
        bool explicaMas,
        string provider,
        string model) =>
        new(key, label, description, status, socratic, explicaMas, provider, model);

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

    public void AddDocument(Guid id, string name, string origin, string status, string statusLabel, DateTime updated,
        string fileName) =>
        this.Docs.Add(new ChatbotDocumentRow
        {
            Id = id,
            Name = name,
            Origin = origin,
            Status = status,
            StatusLabel = statusLabel,
            Updated = updated,
            FileName = fileName,
        });

    public void RemoveDocument(int index)
    {
        if (index >= 0 && index < this.Docs.Count)
        {
            this.Docs.RemoveAt(index);
        }
    }

    public void AddTeamMember(Guid userId, string role) => this.Team.Add(new TeamMemberRow
    {
        UserId = userId,
        Role = role,
    });

    public void RemoveTeamMember(int index)
    {
        if (index >= 0 && index < this.Team.Count)
        {
            this.Team.RemoveAt(index);
        }
    }

    public void RecordQuestion(string question)
    {
        var existing = this.TopQuestions.FirstOrDefault(q => q.Question == question);
        if (existing is not null)
        {
            existing.Count++;
        }
        else
        {
            this.TopQuestions.Add(new GapQuestionRow { Question = question, Count = 1 });
        }
    }
}