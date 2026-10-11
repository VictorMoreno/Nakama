using Nakama.ChatPage.Domain;
using Nakama.ChatPage.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Application.Finder;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public static class ChatPageFinder
{
    public sealed record Query(Guid Id) : IQuery<ChatPageResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, ChatPageResponse>
    {
        public async Task<ChatPageResponse> Handle(Query query, CancellationToken ct)
        {
            ChatPage page = await finder.Execute(query.Id, ct);
            return (ChatPageResponse)page;
        }
    }

    public sealed class Finder(IChatPageRepository repository) : IService
    {
        public async Task<ChatPage> Execute(Guid id, CancellationToken cancellationToken) =>
            await repository.FindById(id, cancellationToken)
                ?? throw ChatPageErrors.NotFound(id.ToString());
    }
}
