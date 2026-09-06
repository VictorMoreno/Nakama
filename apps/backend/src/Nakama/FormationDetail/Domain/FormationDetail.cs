using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

/// <summary>
/// Read entity behind the <c>/formacion/:key</c> route. <see cref="LinkedChatbotLabel"/> is a
/// denormalized copy of <c>Chatbot.Label</c> (resolved via <c>LinkedChatbotKey</c> against this
/// module's own <see cref="ChatbotLabelRef"/> lookup, itself fed by <c>Chatbot</c> events) so
/// the "Basado en la documentación de X" caption never needs a live cross-context join.
/// </summary>
public partial class FormationDetail : IEntity
{
    protected FormationDetail() { } // NHibernate

    public virtual string Key { get; protected set; } = string.Empty;
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual string Audience { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
    public virtual string? LinkedChatbotKey { get; protected set; }
    public virtual string? LinkedChatbotLabel { get; protected set; }

    public virtual List<FormationModuleRow> Modules { get; protected set; } = [];
    public virtual List<FormationMemberRow> Team { get; protected set; } = [];
    public virtual List<string> CompletedModuleIds { get; protected set; } = [];
    public virtual Dictionary<string, int> ExamScores { get; protected set; } = [];
}
