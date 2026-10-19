using Microsoft.Extensions.Options;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotMembers.Application.Searcher;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.Storage;

namespace Nakama.ChatbotSummaries.Application.Searcher;

public static class ChatbotSummarySearcher
{
    public sealed record Query(Guid UserId, bool IsAdmin) : IQuery<IReadOnlyCollection<ChatbotSummaryResponse>>;

    public sealed class Handler(Searcher searcher, IOptions<StorageOptions> storage)
        : IQueryHandler<Query, IReadOnlyCollection<ChatbotSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            IReadOnlyList<ChatbotSummary> chatbots = await searcher.Execute(query.UserId, query.IsAdmin, ct);
            return chatbots.Select(e => ChatbotSummaryResponse.From(e, storage.Value.ChatbotIconsRoute)).ToList();
        }
    }

    public sealed class Searcher(IChatbotSummaryRepository repository, IQueryBus queryBus) : IService
    {
        public async Task<IReadOnlyList<ChatbotSummary>> Execute(Guid userId, bool isAdmin, CancellationToken cancellationToken)
        {
            if (isAdmin)
            {
                return await repository.Search(new ChatbotSummaryListingSpecification(), cancellationToken);
            }

            IReadOnlyCollection<ChatbotMemberResponse> memberships = await queryBus.Ask(
                new ChatbotMemberSearcher.Query(userId),
                cancellationToken);
            ISpecification<ChatbotSummary> specification = new ChatbotSummaryByIdsSpecification(
                memberships.Select(membership => membership.ChatbotId).ToList());

            return await repository.Search(specification, cancellationToken);
        }
    }
}
