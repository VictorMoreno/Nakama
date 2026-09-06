using MediatR;
using Nakama.ChatbotsList.Domain;

namespace Nakama.ChatbotsList.Application.Searcher;

public static class ChatbotSummarySearcher
{
    public sealed record Query : IRequest<IReadOnlyCollection<ChatbotSummaryResponse>>;

    public sealed class Handler(IChatbotSummaryRepository chatbotSummaryRepository)
        : IRequestHandler<Query, IReadOnlyCollection<ChatbotSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var chatbots = await chatbotSummaryRepository.FindAll(ct);
            return chatbots.Select(e => (ChatbotSummaryResponse)e).ToList();
        }
    }
}
