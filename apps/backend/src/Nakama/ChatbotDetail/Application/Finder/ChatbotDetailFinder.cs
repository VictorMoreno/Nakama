using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Application.Finder;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public static class ChatbotDetailFinder
{
    public sealed record Query(Guid Id) : IQuery<ChatbotDetailResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, ChatbotDetailResponse>
    {
        public async Task<ChatbotDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var chatbot = await finder.Execute(query.Id, ct);
            return (ChatbotDetailResponse)chatbot;
        }
    }

    public sealed class Finder(IChatbotDetailRepository repository) : IService
    {
        public async Task<ChatbotDetail> Execute(Guid id, CancellationToken cancellationToken) =>
            await repository.FindById(id, cancellationToken)
                ?? throw ChatbotDetailErrors.NotFound(id.ToString());
    }
}