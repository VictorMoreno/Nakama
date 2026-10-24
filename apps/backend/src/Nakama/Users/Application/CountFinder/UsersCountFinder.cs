using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Users.Domain;

namespace Nakama.Users.Application.CountFinder;

public static class UsersCountFinder
{
    public sealed record Query : IQuery<UsersCountResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, UsersCountResponse>
    {
        public async Task<UsersCountResponse> Handle(Query query, CancellationToken ct) =>
            new(await finder.Execute(ct));
    }

    public sealed class Finder(IUsersRepository repository) : IService
    {
        public Task<int> Execute(CancellationToken cancellationToken) =>
            repository.Count(new UserListingSpecification(), cancellationToken);
    }
}
