namespace Nakama.ConversationAudit.Domain;

public partial class ConversationAuditLog
{
    private ConversationAuditLog(Guid id, Guid userId, Guid botKey, string title, DateTime startedAt)
    {
        this.Id = id;
        this.UserId = userId;
        this.BotKey = botKey;
        this.Title = title;
        this.StartedAt = startedAt;
    }

    public static ConversationAuditLog CreateFromConversationStarted(Guid id, Guid userId, Guid botKey, string title, DateTime startedAt) =>
        new(id, userId, botKey, title, startedAt);

    public void AppendMessage(string question, string answer, string citation, string moreExplanation, string socraticVariant, DateTime recordedAt) =>
        this.Messages.Add(new AuditMessageRow
        {
            Question = question,
            Answer = answer,
            Citation = citation,
            MoreExplanation = moreExplanation,
            SocraticVariant = socraticVariant,
            RecordedAt = recordedAt,
        });
}
