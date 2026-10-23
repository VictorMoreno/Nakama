using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nakama.Analytics.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.LlmProviders.Domain;
using Nakama.QuestionActivity.Application;
using Nakama.Shared.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;
using Test.Modules.Shared;

namespace Test.Api.Analytics;

[TestFixture]
public sealed class QuestionActivityShould : ApiTestCase
{
    private readonly FakeSystemClock _clock = new();

    [SetUp]
    public void ResetClock() => this._clock.Reset();

    protected override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<ISystemClock>(this._clock);

    [Test]
    public async Task count_every_question_asked_in_the_current_week()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = await this.CreateChatbotWithMember(admin);

        await this.Ask(chatbot, admin);
        await this.Ask(chatbot, admin);

        QuestionActivityResponse activity = await this.ReadActivity(admin);
        activity.Weeks.Should().HaveCount(8);
        activity.Weeks[^1].Questions.Should().Be(2);
        activity.QuestionsThisWeek.Should().Be(2);
    }

    [Test]
    public async Task move_the_questions_to_the_week_they_were_asked_in()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = await this.CreateChatbotWithMember(admin);
        await this.Ask(chatbot, admin);
        this._clock.Advance(TimeSpan.FromDays(7));
        await this.Ask(chatbot, admin);

        QuestionActivityResponse activity = await this.ReadActivity(admin, weeks: 2);

        activity.Weeks.Select(week => week.Questions).Should().Equal(1, 1);
    }

    [Test]
    public async Task show_the_questions_of_the_week_in_the_global_analytics()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = await this.CreateChatbotWithMember(admin);
        await this.Ask(chatbot, admin);

        HttpResponseMessage response = await this.Client.Get("/api/analytics", admin);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AnalyticsResponse? analytics = await response.Content.ReadFromJsonAsync<AnalyticsResponse>();
        analytics!.AnsweredThisWeek.Should().Be(1);
    }

    [Test]
    public async Task list_in_the_global_analytics_only_the_chatbots_asked_this_month_with_their_questions()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command asked = await this.CreateChatbotWithMember(admin);
        await this.CreateChatbotWithMember(admin);
        await this.Ask(asked, admin);
        await this.Ask(asked, admin);

        HttpResponseMessage response = await this.Client.Get("/api/analytics", admin);

        AnalyticsResponse? analytics = await response.Content.ReadFromJsonAsync<AnalyticsResponse>();
        analytics!.UsageByBot.Should().ContainSingle().Which.Should().Be(new UsageByBotRow(asked.Id, 2));
    }

    [TestCase(0)]
    [TestCase(27)]
    public async Task reject_a_number_of_weeks_out_of_range(int weeks)
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.Get($"/api/analytics/question-activity?weeks={weeks}", admin);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.question_activity.invalid_weeks");
    }

    [Test]
    public async Task refuse_the_activity_to_regular_users_and_to_anonymous_callers()
    {
        string member = await this.SignIn(isAdmin: false);

        (await this.Client.Get("/api/analytics/question-activity", member)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.Get("/api/analytics/question-activity", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<ChatbotCreator.Command> CreateChatbotWithMember(string adminToken)
    {
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, adminToken);

        return chatbot;
    }

    private async Task Ask(ChatbotCreator.Command chatbot, string accessToken)
    {
        this.Llm.Answer("ok");
        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id, Guid.NewGuid(), WordMother.Random(), WordMother.Random(), accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await response.Content.ReadAsStringAsync();
    }

    private async Task<QuestionActivityResponse> ReadActivity(string accessToken, int? weeks = null)
    {
        string path = weeks is null ? "/api/analytics/question-activity" : $"/api/analytics/question-activity?weeks={weeks}";
        HttpResponseMessage response = await this.Client.Get(path, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<QuestionActivityResponse>())!;
    }
}
