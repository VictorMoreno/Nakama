using MediatR;
using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Domain.Errors;

namespace Nakama.ChatbotDetail.Application.Finder;

public static class ChatbotDetailFinder
{
    public sealed record Query(string Key) : IRequest<ChatbotDetailResponse>;

    public sealed class Handler(IChatbotDetailRepository chatbotDetailRepository) : IRequestHandler<Query, ChatbotDetailResponse>
    {
        public async Task<ChatbotDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var chatbot = await chatbotDetailRepository.FindByKey(query.Key, ct)
                ?? throw ChatbotDetailErrors.NotFound(query.Key);
            return (ChatbotDetailResponse)chatbot;
        }
    }
}
