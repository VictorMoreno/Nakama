using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nakama.Shared.Domain;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;

namespace Test.Api.Analytics;

[TestFixture]
public sealed class UserActivityShould : ApiTestCase
{
    private readonly FakeSystemClock _clock = new();

    [SetUp]
    public void ResetClock() => this._clock.Reset();

    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<ISystemClock>(this._clock);

    [Test]
    public async Task show_the_login_of_an_administrator_as_the_activity_of_today()
    {
        string admin = await this.SignIn(isAdmin: true);

        UserActivityResponse activity = await this.ReadActivity(admin);

        activity.Days.Should().HaveCount(30);
        activity.Days[^1].Logins.Should().Be(1);
        activity.Days[^1].ActiveUsers.Should().Be(1);
        activity.ActiveToday.Should().Be(1);
        activity.ActiveLast7Days.Should().Be(1);
        activity.ActiveLast30Days.Should().Be(1);
    }

    [Test]
    public async Task return_as_many_days_as_requested()
    {
        string admin = await this.SignIn(isAdmin: true);

        UserActivityResponse activity = await this.ReadActivity(admin, days: 7);

        activity.Days.Should().HaveCount(7);
        activity.ActiveLast30Days.Should().Be(1);
    }

    [TestCase(0)]
    [TestCase(91)]
    public async Task reject_a_number_of_days_out_of_range(int days)
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.Get($"/api/analytics/user-activity?days={days}", admin);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.user_activity.invalid_days");
    }

    [Test]
    public async Task refuse_the_activity_to_regular_users_and_to_anonymous_callers()
    {
        string member = await this.SignIn(isAdmin: false);

        (await this.Client.Get("/api/analytics/user-activity", member)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.Get("/api/analytics/user-activity", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task count_the_active_users_of_the_analytics_from_the_same_activity()
    {
        string admin = await this.SignIn(isAdmin: true);
        await this.SignIn(isAdmin: false);
        (await this.Client.CreateChatbot(ChatbotCreatorCommandMother.Random(), admin)).StatusCode.Should().Be(HttpStatusCode.Created);

        HttpResponseMessage response = await this.Client.Get("/api/analytics", admin);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"activeUsersCount\":2");
    }

    [Test]
    public async Task not_record_a_session_activity_when_the_session_is_renewed_within_the_interval()
    {
        SessionCookie session = await this.LogInAnAdministrator();

        HttpResponseMessage refresh = await this.Client.Refresh(session.Value);

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        (await this.RecordedTypes()).Should().Equal(UserActivityTypes.Login);
    }

    [Test]
    public async Task record_a_session_activity_when_the_session_is_renewed_after_the_interval()
    {
        SessionCookie session = await this.LogInAnAdministrator();
        this._clock.Advance(TimeSpan.FromHours(2));

        HttpResponseMessage refresh = await this.Client.Refresh(session.Value);

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        (await this.RecordedTypes()).Should().BeEquivalentTo(UserActivityTypes.Login, UserActivityTypes.Session);
    }

    [Test]
    public async Task record_a_single_session_activity_for_several_renewals_within_the_same_interval()
    {
        SessionCookie session = await this.LogInAnAdministrator();
        this._clock.Advance(TimeSpan.FromHours(2));
        SessionCookie renewed = SessionCookie.RequiredFrom(await this.Client.Refresh(session.Value));
        this._clock.Advance(TimeSpan.FromMinutes(10));

        await this.Client.Refresh(renewed.Value);

        (await this.RecordedTypes()).Should().BeEquivalentTo(UserActivityTypes.Login, UserActivityTypes.Session);
    }

    private async Task<SessionCookie> LogInAnAdministrator()
    {
        UserCredentials admin = await this.RegisterUser(isAdmin: true);

        return SessionCookie.RequiredFrom(await this.Client.LogIn(admin));
    }

    private async Task<UserActivityResponse> ReadActivity(string accessToken, int? days = null)
    {
        string path = days is null ? "/api/analytics/user-activity" : $"/api/analytics/user-activity?days={days}";
        HttpResponseMessage response = await this.Client.Get(path, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<UserActivityResponse>())!;
    }

    private async Task<IReadOnlyList<string>> RecordedTypes()
    {
        await using AsyncServiceScope scope = this.Services.CreateAsyncScope();
        IUserActivityRepository repository = scope.ServiceProvider.GetRequiredService<IUserActivityRepository>();
        IReadOnlyList<UserActivityEntry> entries = await repository.Search(
            new UserActivityBetweenSpecification(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        return entries.Select(entry => entry.Type).ToList();
    }
}
