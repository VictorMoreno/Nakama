using Nakama.Authentication.Application.Refresher;
using Test.Modules.Authentication.Domain;

namespace Test.Modules.Authentication.Application;

public static class LogRefresherQueryMother
{
    public static LogRefresher.Query Random() => With();

    public static LogRefresher.Query WithoutToken() => new(null);

    public static LogRefresher.Query With(string? refreshToken = null) =>
        new(refreshToken ?? RefreshTokenMother.RandomValue());
}
