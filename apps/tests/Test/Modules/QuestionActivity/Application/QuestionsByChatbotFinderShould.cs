using AwesomeAssertions;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.QuestionActivity.Domain;
using Test.Modules.QuestionActivity.Domain;

namespace Test.Modules.QuestionActivity.Application;

[TestFixture]
public sealed class QuestionsByChatbotFinderShould : QuestionActivityUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private QuestionsByChatbotFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new QuestionsByChatbotFinder.Handler(
            new(this.QuestionRepository.Object, this.Clock.Object, this.QuestionOptions));
        this.ShouldBeNow(Now);
        this.ShouldFindQuestions([]);
    }

    [Test]
    public async Task return_nothing_when_no_question_was_asked_this_month()
    {
        QuestionsByChatbotResponse response = await this._handler.Handle(new QuestionsByChatbotFinder.Query(), CancellationToken.None);

        response.Chatbots.Should().BeEmpty();
    }

    [Test]
    public async Task count_the_questions_of_each_chatbot_from_the_most_to_the_least_asked()
    {
        Guid quiet = Guid.NewGuid();
        Guid busy = Guid.NewGuid();
        this.ShouldFindQuestions(
        [
            QuestionActivityEntryMother.With(quiet, occurredAt: Now.AddDays(-3)),
            QuestionActivityEntryMother.With(busy, occurredAt: Now.AddDays(-2)),
            QuestionActivityEntryMother.With(busy, occurredAt: Now.AddDays(-1)),
            QuestionActivityEntryMother.With(busy, occurredAt: Now)
        ]);

        QuestionsByChatbotResponse response = await this._handler.Handle(new QuestionsByChatbotFinder.Query(), CancellationToken.None);

        response.Chatbots.Should().Equal(new ChatbotQuestionsRow(busy, 3), new ChatbotQuestionsRow(quiet, 1));
    }

    [Test]
    public async Task break_the_ties_by_chatbot_so_that_the_order_is_stable()
    {
        Guid first = new("00000000-0000-0000-0000-000000000001");
        Guid second = new("00000000-0000-0000-0000-000000000002");
        this.ShouldFindQuestions(
        [
            QuestionActivityEntryMother.With(second, occurredAt: Now),
            QuestionActivityEntryMother.With(first, occurredAt: Now)
        ]);

        QuestionsByChatbotResponse response = await this._handler.Handle(new QuestionsByChatbotFinder.Query(), CancellationToken.None);

        response.Chatbots.Select(row => row.BotKey).Should().Equal(first, second);
    }

    [Test]
    public async Task look_from_the_start_of_the_month_to_the_end_of_today_in_the_configured_time_zone()
    {
        await this._handler.Handle(new QuestionsByChatbotFinder.Query(), CancellationToken.None);

        QuestionActivityBetweenSpecification search = this.Searches.Should().ContainSingle().Subject;
        search.FromInclusive.Should().Be(new DateTime(2026, 9, 30, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 22, 22, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task start_the_month_in_the_configured_time_zone_and_not_in_utc()
    {
        this.ShouldBeNow(new DateTime(2026, 9, 30, 22, 30, 0, DateTimeKind.Utc));

        await this._handler.Handle(new QuestionsByChatbotFinder.Query(), CancellationToken.None);

        QuestionActivityBetweenSpecification search = this.Searches.Should().ContainSingle().Subject;
        search.FromInclusive.Should().Be(new DateTime(2026, 9, 30, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 1, 22, 0, 0, DateTimeKind.Utc));
    }
}
