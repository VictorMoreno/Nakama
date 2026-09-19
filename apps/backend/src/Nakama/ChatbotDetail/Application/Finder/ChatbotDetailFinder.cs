using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Application.Finder;

public static class ChatbotDetailFinder
{
    public sealed record Query(Guid Id) : IQuery<ChatbotDetailResponse>;

    public sealed class Handler(IChatbotDetailRepository chatbotDetailRepository)
        : IQueryHandler<Query, ChatbotDetailResponse>
    {
        public async Task<ChatbotDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var chatbot = await chatbotDetailRepository.FindById(query.Id, ct)
                          ?? throw ChatbotDetailErrors.NotFound(query.Id.ToString());
            return (ChatbotDetailResponse)chatbot;
        }
    }
}