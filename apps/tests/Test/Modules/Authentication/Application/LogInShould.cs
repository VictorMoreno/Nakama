using AwesomeAssertions;
using Nakama.Authentication.Application;
using Nakama.Authentication.Application.Authenticator;
using Nakama.Authentication.Domain;
using Nakama.Users.Application;
using Nakama.Users.Domain.Errors;
using Test.Modules.Authentication.Domain;
using Test.Modules.Users.Application;

namespace Test.Modules.Authentication.Application;

[TestFixture]
public sealed class LogInShould : AuthenticationUnitTestCase
{
    private LogIn.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new LogIn.Handler(new(
            this.QueryBus.Object,
            this.RefreshTokens.Object,
            this.Clock.Object,
            this.TokenGenerator.Object,
            this.Jwt));
    }

    [Test]
    public async Task return_the_access_token_and_the_user_when_credentials_are_valid()
    {
        UserResponse user = UserResponseMother.Random();
        Token token = TokenMother.Random();
        this.ShouldVerifyCredentials(user);
        this.ShouldGenerateAccessToken(token);

        AuthenticationResponse response = await this._handler.Handle(LogInQueryMother.Random(), CancellationToken.None);

        response.Token.AccessToken.Should().Be(token.Value);
        response.Token.ExpiresAt.Should().Be(token.ExpiresAt);
        response.Token.User.Should().Be(user);
    }

    [Test]
    public async Task generate_the_access_token_with_the_data_of_the_verified_user()
    {
        UserResponse user = UserResponseMother.WithAdmin();
        this.ShouldVerifyCredentials(user);
        this.ShouldGenerateAccessToken(TokenMother.Random());

        await this._handler.Handle(LogInQueryMother.Random(), CancellationToken.None);

        this.ShouldHaveGeneratedAccessTokenFor(user);
    }

    [Test]
    public async Task save_the_returned_refresh_token_with_the_configured_lifetime()
    {
        DateTime now = DateTime.UtcNow;
        UserResponse user = UserResponseMother.Random();
        this.ShouldBeNow(now);
        this.ShouldVerifyCredentials(user);
        this.ShouldGenerateAccessToken(TokenMother.Random());

        AuthenticationResponse response = await this._handler.Handle(LogInQueryMother.Random(), CancellationToken.None);

        DateTime expiresAt = now.AddDays(this.Jwt.Value.RefreshTokenDays);
        response.RefreshTokenExpiresAt.Should().Be(expiresAt);
        this.ShouldHaveSavedRefreshToken(response.RefreshToken, user.Id, expiresAt);
    }

    [Test]
    public async Task fail_without_issuing_tokens_when_credentials_are_invalid()
    {
        this.ShouldRejectCredentials();

        Func<Task> act = async () => await this._handler.Handle(LogInQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidCredentialsError>();
        this.ShouldNotHaveGeneratedAnyAccessToken();
        this.ShouldNotHaveSavedAnyRefreshToken();
    }
}
