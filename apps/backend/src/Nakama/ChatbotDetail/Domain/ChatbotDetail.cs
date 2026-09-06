using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Domain;

/// <summary>
/// Read entity behind the <c>/chatbots/:key</c> route - a single entity covers all 4 tabs
/// (General, Documents, Team, Analytics) rather than one per tab, per the plan's "one read
/// model per route, not per tab" rule. <see cref="Apply(ConversationExchangeAppended)"/> is the
/// cross-aggregate case: it derives the Analytics tab's "most repeated questions" from
/// <c>Conversation</c> events. There's no "was this answer good" signal on a conversation
/// exchange yet, so <see cref="Gaps"/> stays empty for this pass rather than faking a heuristic.
/// </summary>
public partial class ChatbotDetail : IEntity
{
    protected ChatbotDetail() { } // NHibernate

    public virtual string Key { get; protected set; } = string.Empty;
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual DateTime LastUsed { get; protected set; }
    public virtual string Context { get; protected set; } = string.Empty;
    public virtual bool General { get; protected set; }
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
    public virtual bool Socratic { get; protected set; }
    public virtual bool ExplicaMas { get; protected set; }
    public virtual string Provider { get; protected set; } = string.Empty;

    public virtual List<ChatbotDocumentRow> Docs { get; protected set; } = [];
    public virtual List<TeamMemberRow> Team { get; protected set; } = [];
    public virtual List<GapQuestionRow> Gaps { get; protected set; } = [];
    public virtual List<GapQuestionRow> TopQuestions { get; protected set; } = [];
    public virtual List<StatTileRow> Stats { get; protected set; } = [];
}
