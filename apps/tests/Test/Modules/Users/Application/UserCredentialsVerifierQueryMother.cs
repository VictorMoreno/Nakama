using Nakama.Users.Application.CredentialsVerifier;
using Test.Modules.Shared;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

public static class UserCredentialsVerifierQueryMother
{
    public static UserCredentialsVerifier.Query Random() => With();

    public static UserCredentialsVerifier.Query With(string? email = null, string? password = null) =>
        new(email ?? UserMother.RandomEmail().Value, password ?? WordMother.Random());
}
