using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Domain;

namespace Nakama.ChatbotsList.Application.Searcher;

public static class ChatbotSummarySearcher
{
    public sealed record Query : IQuery<IReadOnlyCollection<ChatbotSummaryResponse>>;

    public sealed class Handler(IChatbotSummaryRepository chatbotSummaryRepository)
        : IQueryHandler<Query, IReadOnlyCollection<ChatbotSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var chatbots = await chatbotSummaryRepository.FindAll(ct);
            return chatbots.Select(e => (ChatbotSummaryResponse)e).ToList();
        }
    }
}
