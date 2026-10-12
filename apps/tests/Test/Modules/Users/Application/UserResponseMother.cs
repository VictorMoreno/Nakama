using Nakama.Users.Application;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

public static class UserResponseMother
{
    public static UserResponse Random() => With();

    public static UserResponse WithAdmin() => With(isAdmin: true);

    public static UserResponse With(Guid? id = null, string? email = null, string? name = null, bool? isAdmin = null) =>
        new(
            id ?? UserMother.RandomId().Value,
            email ?? UserMother.RandomEmail().Value,
            name ?? "Test User",
            isAdmin ?? false);
}
