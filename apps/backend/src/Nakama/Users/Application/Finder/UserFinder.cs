using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Application.Finder;

public static class UserFinder
{
    public sealed record Query(Guid Id) : IQuery<UserResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, UserResponse>
    {
        public async Task<UserResponse> Handle(Query query, CancellationToken ct)
        {
            User user = await finder.Execute(UserId.From(query.Id), ct);
            return (UserResponse)user;
        }
    }

    public sealed class Finder(IUsersRepository repository) : IService
    {
        public async Task<User> Execute(UserId id, CancellationToken ct) =>
            await repository.FindById(id, ct) ?? throw UserErrors.NotFound(id.ToString());
    }
}
