using Nakama.ChatbotMembers.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.ChatbotMembers.Application.Searcher;

public static class ChatbotMemberSearcher
{
    public sealed record Query(Guid UserId) : IQuery<IReadOnlyCollection<ChatbotMemberResponse>>;

    public sealed class Handler(Searcher searcher) : IQueryHandler<Query, IReadOnlyCollection<ChatbotMemberResponse>>
    {
        public async Task<IReadOnlyCollection<ChatbotMemberResponse>> Handle(Query query, CancellationToken ct)
        {
            IReadOnlyList<ChatbotMember> members = await searcher.Execute(query.UserId, ct);
            return members.Select(m => (ChatbotMemberResponse)m).ToList();
        }
    }

    public sealed class Searcher(IChatbotMemberRepository repository) : IService
    {
        public Task<IReadOnlyList<ChatbotMember>> Execute(Guid userId, CancellationToken cancellationToken) =>
            repository.Search(new ChatbotMemberByUserSpecification(userId), cancellationToken);
    }
}
