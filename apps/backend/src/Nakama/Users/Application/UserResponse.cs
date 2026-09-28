using Nakama.Users.Domain;

namespace Nakama.Users.Application;

public sealed record UserResponse(Guid Id, string Email, string Name, bool IsAdmin)
{
    public static explicit operator UserResponse(User entity) => new(
        entity.Id.Value,
        entity.Email,
        entity.Name,
        entity.IsAdmin);
}
