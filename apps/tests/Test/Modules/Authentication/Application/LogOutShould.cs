using AwesomeAssertions;
using Nakama.Authentication.Application.Closer;
using Nakama.Authentication.Domain;
using Test.Modules.Authentication.Domain;

namespace Test.Modules.Authentication.Application;

[TestFixture]
public sealed class LogOutShould : AuthenticationUnitTestCase
{
    private LogOut.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new LogOut.Handler(new(this.RefreshTokens.Object, this.Clock.Object));
    }

    [Test]
    public async Task revoke_and_save_the_refresh_token()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken token = RefreshTokenMother.Random();
        this.ShouldBeNow(now);
        this.ShouldFindRefreshToken(token);

        await this._handler.Handle(LogOutCommandMother.With(token.Value), CancellationToken.None);

        token.RevokedAt.Should().Be(now);
        this.ShouldHaveSaved(token);
    }

    [Test]
    public async Task do_nothing_when_the_refresh_token_does_not_exist()
    {
        this.ShouldFindRefreshToken(null);

        await this._handler.Handle(LogOutCommandMother.Random(), CancellationToken.None);

        this.ShouldNotHaveSavedAnyRefreshToken();
    }

    [Test]
    public async Task keep_the_original_revocation_date_when_the_refresh_token_was_already_revoked()
    {
        DateTime revokedAt = DateTime.UtcNow.AddHours(-1);
        RefreshToken token = RefreshTokenMother.With(revokedAt: revokedAt);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindRefreshToken(token);

        await this._handler.Handle(LogOutCommandMother.With(token.Value), CancellationToken.None);

        token.RevokedAt.Should().Be(revokedAt);
    }
}
