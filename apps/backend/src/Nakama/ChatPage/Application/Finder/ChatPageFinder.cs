using MediatR;
using Nakama.ChatPage.Domain;
using Nakama.ChatPage.Domain.Errors;

namespace Nakama.ChatPage.Application.Finder;

public static class ChatPageFinder
{
    public sealed record Query(string BotKey) : IRequest<ChatPageResponse>;

    public sealed class Handler(IChatPageRepository chatPageRepository) : IRequestHandler<Query, ChatPageResponse>
    {
        public async Task<ChatPageResponse> Handle(Query query, CancellationToken ct)
        {
            var page = await chatPageRepository.FindByBotKey(query.BotKey, ct)
                ?? throw ChatPageErrors.NotFound(query.BotKey);
            return (ChatPageResponse)page;
        }
    }
}
