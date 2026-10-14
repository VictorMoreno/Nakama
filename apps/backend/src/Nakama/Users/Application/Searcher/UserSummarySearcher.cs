using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Users.Domain;

namespace Nakama.Users.Application.Searcher;

public static class UserSummarySearcher
{
    public sealed record Query : IQuery<IReadOnlyCollection<UserSummaryResponse>>;

    public sealed class Handler(Searcher searcher) : IQueryHandler<Query, IReadOnlyCollection<UserSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<UserSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            IReadOnlyList<User> users = await searcher.Execute(ct);
            return users.Select(u => (UserSummaryResponse)u).ToList();
        }
    }

    public sealed class Searcher(IUsersRepository repository) : IService
    {
        public Task<IReadOnlyList<User>> Execute(CancellationToken cancellationToken) =>
            repository.Search(new UserListingSpecification(), cancellationToken);
    }
}
