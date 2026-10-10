using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Domain;

public partial class ChatbotDetail : IEntity
{
    protected ChatbotDetail() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual DateTime LastUsed { get; protected set; }
    public virtual string Context { get; protected set; } = string.Empty;
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
    public virtual bool Socratic { get; protected set; }
    public virtual bool ExplicaMas { get; protected set; }
    public virtual string Provider { get; protected set; } = string.Empty;
    public virtual string Model { get; protected set; } = string.Empty;

    public virtual List<ChatbotDocumentRow> Docs { get; protected set; } = [];
    public virtual List<TeamMemberRow> Team { get; protected set; } = [];
    public virtual List<GapQuestionRow> Gaps { get; protected set; } = [];
    public virtual List<GapQuestionRow> TopQuestions { get; protected set; } = [];
    public virtual List<StatTileRow> Stats { get; protected set; } = [];
}
