using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.AnswerFeedback.Domain;

public sealed class RatedAnswersSpecification : BaseSpecification<AnswerFeedbackEntry>
{
    protected override Expression<Func<AnswerFeedbackEntry, bool>> CreateExpression() => entry => true;
}
