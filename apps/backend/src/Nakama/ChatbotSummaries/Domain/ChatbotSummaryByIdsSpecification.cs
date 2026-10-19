using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotSummaries.Domain;

public sealed class ChatbotSummaryByIdsSpecification : BaseSpecification<ChatbotSummary>
{
    public ChatbotSummaryByIdsSpecification(IReadOnlyCollection<Guid> ids)
    {
        this.Ids = ids;
        this.ApplyOrderBy(chatbot => chatbot.Label);
    }

    public IReadOnlyCollection<Guid> Ids { get; }

    protected override Expression<Func<ChatbotSummary, bool>> CreateExpression()
    {
        if (this.Ids.Count == 0)
        {
            return PredicateBuilder.New<ChatbotSummary>(false);
        }

        return chatbot => this.Ids.Contains(chatbot.Id);
    }
}
