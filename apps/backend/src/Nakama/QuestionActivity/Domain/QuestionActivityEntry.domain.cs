namespace Nakama.QuestionActivity.Domain;

public partial class QuestionActivityEntry
{
    public QuestionActivityEntry(Guid id, Guid botKey, Guid userId, DateTime occurredAt)
    {
        this.Id = id;
        this.BotKey = botKey;
        this.UserId = userId;
        this.OccurredAt = occurredAt;
    }

    public static QuestionActivityEntry Create(Guid id, Guid botKey, Guid userId, DateTime occurredAt) =>
        new(id, botKey, userId, occurredAt);
}
