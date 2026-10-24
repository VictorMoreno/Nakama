using Microsoft.Extensions.Options;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotMembers.Application.Searcher;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Storage;

namespace Nakama.ChatbotSummaries.Application.PageSearcher;

public static class ChatbotSummaryPageSearcher
{
    public sealed record Query(Guid UserId, bool IsAdmin, int Page, int? PageSize, string? Search, string? Status, bool Archived = false)
        : IQuery<PagedResponse<ChatbotSummaryResponse>>;

    public sealed class Handler(Searcher searcher, IOptions<StorageOptions> storage, IOptions<PaginationOptions> pagination)
        : IQueryHandler<Query, PagedResponse<ChatbotSummaryResponse>>
    {
        public async Task<PagedResponse<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            PageRequest pageRequest = PageRequest.From(
                query.Page,
                query.PageSize,
                pagination.Value.DefaultPageSize,
                pagination.Value.MaxPageSize);
            PagedResult<ChatbotSummary> result = await searcher.Execute(query, pageRequest, ct);

            return PagedResponse<ChatbotSummaryResponse>.From(
                result.Items.Select(e => ChatbotSummaryResponse.From(e, storage.Value.ChatbotIconsRoute)).ToList(),
                result.TotalCount,
                pageRequest);
        }
    }

    public sealed class Searcher(IChatbotSummaryRepository chatbotSummaryRepository, IQueryBus queryBus) : IService
    {
        public async Task<PagedResult<ChatbotSummary>> Execute(Query query, PageRequest pageRequest, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Guid>? chatbotIds = null;

            if (!query.IsAdmin)
            {
                IReadOnlyCollection<ChatbotMemberResponse> memberships = await queryBus.Ask(
                    new ChatbotMemberSearcher.Query(query.UserId),
                    cancellationToken);
                chatbotIds = memberships.Select(membership => membership.ChatbotId).ToList();
            }

            return await chatbotSummaryRepository.SearchPage(
                new ChatbotSummaryPageSpecification(
                    chatbotIds,
                    query.Search,
                    query.Status,
                    pageRequest.Skip,
                    pageRequest.PageSize,
                    query.IsAdmin && query.Archived),
                cancellationToken);
        }
    }
}
