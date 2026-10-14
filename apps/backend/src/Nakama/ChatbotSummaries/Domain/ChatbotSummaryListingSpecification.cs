using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotSummaries.Domain;

public sealed class ChatbotSummaryListingSpecification : BaseSpecification<ChatbotSummary>
{
    public ChatbotSummaryListingSpecification() => this.ApplyOrderBy(chatbot => chatbot.Label);

    protected override Expression<Func<ChatbotSummary, bool>> CreateExpression() => PredicateBuilder.New<ChatbotSummary>();
}
