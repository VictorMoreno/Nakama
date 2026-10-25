using Nakama.Shared.Domain;

namespace Nakama.QuestionActivity.Domain;

public partial class QuestionActivityEntry : IEntity
{
    protected QuestionActivityEntry() { }

    public Guid Id { get; protected set; }
    public Guid BotKey { get; protected set; }
    public Guid UserId { get; protected set; }
    public DateTime OccurredAt { get; protected set; }
}
