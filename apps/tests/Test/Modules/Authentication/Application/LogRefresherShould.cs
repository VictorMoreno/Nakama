using AwesomeAssertions;
using Nakama.Authentication.Application;
using Nakama.Authentication.Application.Refresher;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Domain.Errors;
using Nakama.Users.Application;
using Test.Modules.Authentication.Domain;
using Test.Modules.Users.Application;

namespace Test.Modules.Authentication.Application;

[TestFixture]
public sealed class LogRefresherShould : AuthenticationUnitTestCase
{
    private LogRefresher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new LogRefresher.Handler(new(
            this.QueryBus.Object,
            this.RefreshTokens.Object,
            this.Clock.Object,
            this.TokenGenerator.Object,
            this.Jwt));
    }

    [Test]
    public async Task rotate_the_refresh_token_and_return_new_tokens()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken current = RefreshTokenMother.Random();
        Token token = TokenMother.Random();
        this.ShouldBeNow(now);
        this.ShouldFindRefreshToken(current);
        UserResponse user = UserResponseMother.With(id: current.UserId);
        this.ShouldFindUser(user);
        this.ShouldGenerateAccessToken(user, token);
        this.ShouldRotate(current, true, now);

        AuthenticationResponse response = await this._handler.Handle(
            LogRefresherQueryMother.With(current.Value),
            CancellationToken.None);

        response.Token.AccessToken.Should().Be(token.Value);
        response.RefreshToken.Should().NotBe(current.Value);
        response.RefreshTokenExpiresAt.Should().Be(now.AddDays(this.Jwt.Value.RefreshTokenDays));
        this.ShouldHaveRotated(current, now);
    }

    [Test]
    public async Task generate_the_access_token_with_the_data_of_the_current_user()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken current = RefreshTokenMother.Random();
        UserResponse user = UserResponseMother.With(id: current.UserId, isAdmin: true);
        this.ShouldBeNow(now);
        this.ShouldFindRefreshToken(current);
        this.ShouldFindUser(user);
        this.ShouldGenerateAccessToken(user, TokenMother.Random());
        this.ShouldRotate(current, true, now);

        await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        this.ShouldHaveGeneratedAccessTokenFor(user);
    }

    [Test]
    public async Task fail_without_searching_anything_when_there_is_no_refresh_token()
    {
        Func<Task> act = async () => await this._handler.Handle(LogRefresherQueryMother.WithoutToken(), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveSearchedForAnyRefreshToken();
        this.ShouldNotHaveRotated();
        this.ShouldNotHaveRevokedAnyToken();
    }

    [Test]
    public async Task fail_without_searching_anything_when_the_refresh_token_is_blank()
    {
        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(" "), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveSearchedForAnyRefreshToken();
        this.ShouldNotHaveRotated();
        this.ShouldNotHaveRevokedAnyToken();
    }

    [Test]
    public async Task fail_when_the_refresh_token_does_not_exist()
    {
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldNotFindRefreshToken();

        Func<Task> act = async () => await this._handler.Handle(LogRefresherQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveRotated();
    }

    [Test]
    public async Task fail_when_the_refresh_token_is_expired()
    {
        RefreshToken current = RefreshTokenMother.Expired();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindRefreshToken(current);

        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveRotated();
        this.ShouldNotHaveRevokedAnyToken();
    }

    [Test]
    public async Task fail_when_the_refresh_token_is_revoked()
    {
        RefreshToken current = RefreshTokenMother.Revoked();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindRefreshToken(current);

        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveRotated();
        this.ShouldNotHaveRevokedAnyToken();
    }

    [Test]
    public async Task revoke_every_token_of_the_user_when_a_rotated_refresh_token_is_reused()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken current = RefreshTokenMother.Rotated();
        this.ShouldBeNow(now);
        this.ShouldFindRefreshToken(current);

        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldHaveRevokedAllTokensOf(current.UserId, now);
        this.ShouldNotHaveRotated();
    }

    [Test]
    public async Task fail_when_the_user_of_the_refresh_token_no_longer_exists()
    {
        RefreshToken current = RefreshTokenMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindRefreshToken(current);
        this.ShouldNotFindUser(current.UserId);

        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldNotHaveRotated();
    }

    [Test]
    public async Task revoke_every_token_of_the_user_when_the_rotation_loses_the_race()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken current = RefreshTokenMother.Random();
        UserResponse user = UserResponseMother.With(id: current.UserId);
        this.ShouldBeNow(now);
        this.ShouldFindRefreshToken(current);
        this.ShouldFindUser(user);
        this.ShouldGenerateAccessToken(user, TokenMother.Random());
        this.ShouldRotate(current, false, now);

        Func<Task> act = async () =>
            await this._handler.Handle(LogRefresherQueryMother.With(current.Value), CancellationToken.None);

        await act.Should().ThrowAsync<AuthInvalidRefreshTokenError>();
        this.ShouldHaveRevokedAllTokensOf(current.UserId, now);
    }
}
