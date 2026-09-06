using Nakama.Shared.Domain;

namespace Nakama.ChatbotsList.Domain;

/// <summary>
/// Read entity behind the <c>/chatbots</c> listing route. Fed entirely by <c>Chatbot</c>
/// domain events (see <see cref="Infrastructure.Projections.ChatbotsListProjection"/>) -
/// denormalizes doc/team counts instead of exposing the full arrays, which the list view
/// doesn't need. Implements <see cref="IEntity"/> like any domain entity, but is not an
/// <c>AggregateRoot</c>: it never raises its own events, it only consumes events raised by
/// the <c>Chatbot</c> aggregate.
/// </summary>
public partial class ChatbotSummary : IEntity
{
    protected ChatbotSummary() { } // NHibernate

    public virtual string Key { get; protected set; } = string.Empty;
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual DateTime LastUsed { get; protected set; }
    public virtual int DocsCount { get; protected set; }
    public virtual int TeamCount { get; protected set; }
    public virtual bool General { get; protected set; }
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
}
