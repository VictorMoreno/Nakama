using Nakama.Authentication.Application.Authenticator;
using Test.Modules.Shared;
using Test.Modules.Users.Domain;

namespace Test.Modules.Authentication.Application;

public static class LogInQueryMother
{
    public static LogIn.Query Random() => With();

    public static LogIn.Query With(string? email = null, string? password = null) =>
        new(email ?? UserMother.RandomEmail().Value, password ?? WordMother.Random());
}
