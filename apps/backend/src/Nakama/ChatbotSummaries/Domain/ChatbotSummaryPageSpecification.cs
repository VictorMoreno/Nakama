using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.ChatbotSummaries.Domain;

public sealed class ChatbotSummaryPageSpecification : BaseSpecification<ChatbotSummary>
{
    public ChatbotSummaryPageSpecification(
        IReadOnlyCollection<Guid>? ids,
        string? search,
        string? status,
        int skip = 0,
        int? take = null,
        bool archived = false)
    {
        this.Archived = archived;
        this.Ids = ids;
        this.Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();
        this.Status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        this.ApplyOrderBy(chatbot => chatbot.Label);

        if (take is not null)
        {
            this.ApplyPaging(skip, take.Value);
        }
    }

    public IReadOnlyCollection<Guid>? Ids { get; }

    public string? Search { get; }

    public string? Status { get; }

    public bool Archived { get; }

    protected override Expression<Func<ChatbotSummary, bool>> CreateExpression()
    {
        bool archived = this.Archived;
        Expression<Func<ChatbotSummary, bool>> criteria = chatbot => chatbot.Archived == archived;

        if (this.Ids is not null)
        {
            IReadOnlyCollection<Guid> ids = this.Ids;
            criteria = criteria.And(ids.Count == 0
                ? PredicateBuilder.New<ChatbotSummary>(false)
                : chatbot => ids.Contains(chatbot.Id));
        }

        if (this.Status is not null)
        {
            string status = this.Status;
            criteria = criteria.And(chatbot => chatbot.Status == status);
        }

        if (this.Search is not null)
        {
            string search = this.Search;
            criteria = criteria.And(chatbot =>
                chatbot.Label.ToLower().Contains(search) || chatbot.Description.ToLower().Contains(search));
        }

        return criteria;
    }
}
