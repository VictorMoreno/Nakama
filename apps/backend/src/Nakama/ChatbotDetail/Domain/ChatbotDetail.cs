using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Domain;

public partial class ChatbotDetail : IEntity
{
    protected ChatbotDetail() { } // NHibernate

    public Guid Id { get; protected set; }
    public string Label { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = string.Empty;
    public bool Archived { get; protected set; }
    public string? IconUrl { get; protected set; }
    public bool Socratic { get; protected set; }
    public bool ExplicaMas { get; protected set; }
    public string Provider { get; protected set; } = string.Empty;
    public string Model { get; protected set; } = string.Empty;

    public List<ChatbotDocumentRow> Docs { get; protected set; } = [];
    public List<TeamMemberRow> Team { get; protected set; } = [];
}
