using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.AnswerFeedback.Domain;

public sealed class UnhelpfulAnswersOfChatbotBetweenSpecification : BaseSpecification<AnswerFeedbackEntry>
{
    public UnhelpfulAnswersOfChatbotBetweenSpecification(Guid botKey, DateTime fromInclusive, DateTime toExclusive)
    {
        this.BotKey = botKey;
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.RatedAt);
    }

    public Guid BotKey { get; }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    protected override Expression<Func<AnswerFeedbackEntry, bool>> CreateExpression() =>
        entry => !entry.Useful
            && entry.BotKey == this.BotKey
            && entry.RatedAt >= this.FromInclusive
            && entry.RatedAt < this.ToExclusive;
}
