using Test.Modules.Users.Domain;
using Nakama.Users.Application.Creator;

namespace Test.Modules.Users.Application;

public static class UserCreatorCommandMother
{
    public static UserCreator.Command Random() => With();

    public static UserCreator.Command With(
        Guid? id = null,
        string? email = null,
        string? name = null,
        bool? isAdmin = null,
        string? initialPassword = null) =>
        new(
            id ?? UserMother.RandomId().Value,
            email ?? UserMother.RandomEmail().Value,
            name ?? "Test User",
            isAdmin ?? false,
            initialPassword ?? "Valid-Password-1");
}
