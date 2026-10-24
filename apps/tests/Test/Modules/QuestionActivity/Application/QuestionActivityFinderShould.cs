using AwesomeAssertions;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.QuestionActivity.Domain;
using Nakama.QuestionActivity.Domain.Errors;
using Test.Modules.QuestionActivity.Domain;

namespace Test.Modules.QuestionActivity.Application;

[TestFixture]
public sealed class QuestionActivityFinderShould : QuestionActivityUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private QuestionActivityFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new QuestionActivityFinder.Handler(
            new(this.QuestionRepository.Object, this.Calendar, this.QuestionOptions));
        this.ShouldBeNow(Now);
        this.ShouldFindQuestions([]);
    }

    [Test]
    public async Task return_a_row_per_requested_week_from_the_oldest_to_the_current_one_even_without_questions()
    {
        QuestionActivityResponse response = await this._handler.Handle(new QuestionActivityFinder.Query(3), CancellationToken.None);

        response.Weeks.Select(week => week.WeekStart).Should().Equal("2026-10-05", "2026-10-12", "2026-10-19");
        response.Weeks.Should().OnlyContain(week => week.Questions == 0);
        response.QuestionsThisWeek.Should().Be(0);
    }

    [Test]
    public async Task start_every_week_on_monday()
    {
        this.ShouldBeNow(new DateTime(2026, 10, 25, 20, 0, 0, DateTimeKind.Utc));

        QuestionActivityResponse response = await this._handler.Handle(new QuestionActivityFinder.Query(1), CancellationToken.None);

        response.Weeks.Should().ContainSingle().Which.WeekStart.Should().Be("2026-10-19");
    }

    [Test]
    public async Task count_the_questions_of_each_week()
    {
        this.ShouldFindQuestions(
        [
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 12, 9, 0, 0, DateTimeKind.Utc)),
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc)),
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 19, 9, 0, 0, DateTimeKind.Utc)),
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc)),
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 22, 9, 0, 0, DateTimeKind.Utc))
        ]);

        QuestionActivityResponse response = await this._handler.Handle(new QuestionActivityFinder.Query(2), CancellationToken.None);

        response.Weeks.Should().Equal(
            new WeeklyQuestionRow("2026-10-12", 2),
            new WeeklyQuestionRow("2026-10-19", 3));
        response.QuestionsThisWeek.Should().Be(3);
    }

    [Test]
    public async Task assign_the_questions_to_the_week_of_the_configured_time_zone_and_not_to_the_one_in_utc()
    {
        this.ShouldFindQuestions(
        [
            QuestionActivityEntryMother.With(occurredAt: new DateTime(2026, 10, 18, 22, 30, 0, DateTimeKind.Utc))
        ]);

        QuestionActivityResponse response = await this._handler.Handle(new QuestionActivityFinder.Query(2), CancellationToken.None);

        response.Weeks.Should().Equal(
            new WeeklyQuestionRow("2026-10-12", 0),
            new WeeklyQuestionRow("2026-10-19", 1));
    }

    [Test]
    public async Task look_from_the_start_of_the_oldest_week_to_the_end_of_today_in_the_configured_time_zone()
    {
        await this._handler.Handle(new QuestionActivityFinder.Query(2), CancellationToken.None);

        QuestionActivityBetweenSpecification search = this.Searches.Should().ContainSingle().Subject;
        search.FromInclusive.Should().Be(new DateTime(2026, 10, 11, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 22, 22, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task use_the_default_number_of_weeks_when_none_is_requested()
    {
        QuestionActivityResponse response = await this._handler.Handle(new QuestionActivityFinder.Query(null), CancellationToken.None);

        response.Weeks.Should().HaveCount(this.QuestionOptions.Value.DefaultWeeks);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(27)]
    public async Task reject_a_number_of_weeks_out_of_range(int weeks)
    {
        Func<Task> act = async () => await this._handler.Handle(new QuestionActivityFinder.Query(weeks), CancellationToken.None);

        await act.Should().ThrowAsync<QuestionActivityInvalidWeeksError>();
        this.Searches.Should().BeEmpty();
    }

    [Test]
    public async Task search_only_the_questions_of_the_given_person()
    {
        Guid userId = Guid.NewGuid();

        await this._handler.Handle(new QuestionActivityFinder.Query(1, userId), CancellationToken.None);

        this.Searches.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    [Test]
    public async Task search_the_questions_of_everyone_when_no_person_is_given()
    {
        await this._handler.Handle(new QuestionActivityFinder.Query(1), CancellationToken.None);

        this.Searches.Should().ContainSingle().Which.UserId.Should().BeNull();
    }
}
