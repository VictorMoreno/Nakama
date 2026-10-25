using Nakama.Shared.Domain;

namespace Nakama.AnswerFeedback.Domain;

public partial class AnswerFeedbackEntry : IEntity
{
    protected AnswerFeedbackEntry() { }

    public Guid Id { get; protected set; }
    public Guid BotKey { get; protected set; }
    public string Question { get; protected set; } = string.Empty;
    public bool Useful { get; protected set; }
    public DateTime RatedAt { get; protected set; }
}
