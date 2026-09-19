using Nakama.ChatPage.Domain;
using Nakama.ChatPage.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Application.Finder;

public static class ChatPageFinder
{
    public sealed record Query(Guid Id) : IQuery<ChatPageResponse>;

    public sealed class Handler(IChatPageRepository chatPageRepository) : IQueryHandler<Query, ChatPageResponse>
    {
        public async Task<ChatPageResponse> Handle(Query query, CancellationToken ct)
        {
            var page = await chatPageRepository.FindById(query.Id, ct)
                ?? throw ChatPageErrors.NotFound(query.Id.ToString());
            return (ChatPageResponse)page;
        }
    }
}
