using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.Authentication.Application;
using Test.Api.Support;

namespace Test.Api.Authentication;

[TestFixture]
public sealed class SessionFlowShould : ApiTestCase
{
    [Test]
    public async Task log_in_with_valid_credentials_and_use_the_access_token_on_a_protected_endpoint()
    {
        UserCredentials admin = await this.RegisterUser(isAdmin: true);

        HttpResponseMessage login = await this.Client.LogIn(admin);

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        TokenResponse? token = await login.Content.ReadFromJsonAsync<TokenResponse>();
        token.Should().NotBeNull();
        token.User.Email.Should().Be(admin.Email);
        token.User.IsAdmin.Should().BeTrue();
        SessionCookie cookie = SessionCookie.RequiredFrom(login);
        cookie.IsHttpOnly.Should().BeTrue();
        cookie.IsSecure.Should().BeTrue();
        cookie.IsSameSiteStrict.Should().BeTrue();
        cookie.IsScopedToTheAuthRoutes.Should().BeTrue();
        HttpResponseMessage users = await this.Client.Get("/api/users", token.AccessToken);
        users.StatusCode.Should().Be(HttpStatusCode.OK);
        (await users.Content.ReadAsStringAsync()).Should().Contain(admin.Email);
    }

    [Test]
    public async Task reject_a_wrong_password_without_setting_any_cookie()
    {
        UserCredentials user = await this.RegisterUser(isAdmin: false);

        HttpResponseMessage login = await this.Client.LogIn(user with { Password = "Wrong-Password-1." });

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ProblemDetails? problem = await login.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Title.Should().Be("nakama.users.invalid_credentials");
        SessionCookie.From(login).Should().BeNull();
    }

    [Test]
    public async Task rotate_the_refresh_token_on_every_refresh_and_issue_a_working_access_token()
    {
        UserCredentials admin = await this.RegisterUser(isAdmin: true);
        SessionCookie first = SessionCookie.RequiredFrom(await this.Client.LogIn(admin));

        HttpResponseMessage refresh = await this.Client.Refresh(first.Value);

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        TokenResponse? token = await refresh.Content.ReadFromJsonAsync<TokenResponse>();
        token.Should().NotBeNull();
        SessionCookie second = SessionCookie.RequiredFrom(refresh);
        second.Value.Should().NotBe(first.Value);
        (await this.Client.Get("/api/users", token.AccessToken)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task close_the_whole_session_when_a_rotated_refresh_token_is_used_again()
    {
        UserCredentials admin = await this.RegisterUser(isAdmin: true);
        SessionCookie first = SessionCookie.RequiredFrom(await this.Client.LogIn(admin));
        SessionCookie second = SessionCookie.RequiredFrom(await this.Client.Refresh(first.Value));

        HttpResponseMessage reuse = await this.Client.Refresh(first.Value);
        HttpResponseMessage afterwards = await this.Client.Refresh(second.Value);

        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await reuse.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.auth.invalid_refresh_token");
        afterwards.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task log_out_revoking_the_refresh_token_and_clearing_the_cookie()
    {
        UserCredentials user = await this.RegisterUser(isAdmin: false);
        SessionCookie cookie = SessionCookie.RequiredFrom(await this.Client.LogIn(user));

        HttpResponseMessage logout = await this.Client.LogOut(cookie.Value);
        HttpResponseMessage refreshAfterwards = await this.Client.Refresh(cookie.Value);

        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        SessionCookie.RequiredFrom(logout).IsExpired.Should().BeTrue();
        refreshAfterwards.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task protect_the_administration_endpoints_by_token_and_by_role()
    {
        UserCredentials member = await this.RegisterUser(isAdmin: false);
        TokenResponse? memberToken = await (await this.Client.LogIn(member)).Content.ReadFromJsonAsync<TokenResponse>();

        HttpResponseMessage withoutToken = await this.Client.Get("/api/users", null);
        HttpResponseMessage asMember = await this.Client.Get("/api/users", memberToken!.AccessToken);

        withoutToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        asMember.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
