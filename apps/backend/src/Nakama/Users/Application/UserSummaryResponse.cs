using Nakama.Users.Domain;

namespace Nakama.Users.Application;

public sealed record UserSummaryResponse(Guid Id, string Name, string Email)
{
    public static explicit operator UserSummaryResponse(User entity) => new(
        entity.Id.Value,
        entity.Name,
        entity.Email);
}
