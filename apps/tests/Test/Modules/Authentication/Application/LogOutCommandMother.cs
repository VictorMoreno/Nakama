using Nakama.Authentication.Application.Closer;
using Test.Modules.Authentication.Domain;

namespace Test.Modules.Authentication.Application;

public static class LogOutCommandMother
{
    public static LogOut.Command Random() => With();

    public static LogOut.Command With(string? refreshToken = null) =>
        new(refreshToken ?? RefreshTokenMother.RandomValue());
}
