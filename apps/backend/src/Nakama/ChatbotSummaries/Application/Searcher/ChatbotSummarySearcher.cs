using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.ChatbotSummaries.Application.Searcher;

public static class ChatbotSummarySearcher
{
    public sealed record Query : IQuery<IReadOnlyCollection<ChatbotSummaryResponse>>;

    public sealed class Handler(Searcher searcher)
        : IQueryHandler<Query, IReadOnlyCollection<ChatbotSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var chatbots = await searcher.Execute(ct);
            return chatbots.Select(e => (ChatbotSummaryResponse)e).ToList();
        }
    }

    public sealed class Searcher(IChatbotSummaryRepository repository) : IService
    {
        public Task<IReadOnlyCollection<ChatbotSummary>> Execute(CancellationToken cancellationToken) =>
            repository.FindAll(cancellationToken);
    }
}
