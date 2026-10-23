using System.Security.Cryptography;
using System.Text;

namespace Nakama.AnswerFeedback.Domain;

public partial class AnswerFeedbackEntry
{
    public AnswerFeedbackEntry(Guid id, Guid botKey, string question, bool useful, DateTime ratedAt)
    {
        this.Id = id;
        this.BotKey = botKey;
        this.Question = question;
        this.Useful = useful;
        this.RatedAt = ratedAt;
    }

    public static Guid IdOf(Guid conversationId, int exchangeIndex) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"{conversationId:N}:{exchangeIndex}"))[..16]);

    public static AnswerFeedbackEntry Create(Guid conversationId, int exchangeIndex, Guid botKey, string question, bool useful, DateTime ratedAt) =>
        new(IdOf(conversationId, exchangeIndex), botKey, question, useful, ratedAt);

    public void Rate(bool useful, DateTime ratedAt)
    {
        this.Useful = useful;
        this.RatedAt = ratedAt;
    }
}
