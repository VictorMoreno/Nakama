using AwesomeAssertions;
using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Finder;
using Nakama.AnswerFeedback.Domain;
using Test.Modules.AnswerFeedback.Domain;

namespace Test.Modules.AnswerFeedback.Application;

[TestFixture]
public sealed class UnhelpfulQuestionsFinderShould : AnswerFeedbackUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private UnhelpfulQuestionsFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UnhelpfulQuestionsFinder.Handler(
            new(this.FeedbackRepository.Object, this.Calendar, this.FeedbackOptions));
        this.ShouldBeNow(Now);
        this.ShouldFindUnhelpfulAnswers([]);
    }

    [Test]
    public async Task return_nothing_when_no_answer_was_rated_as_not_useful()
    {
        UnhelpfulQuestionsResponse response = await this._handler.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        response.Questions.Should().BeEmpty();
    }

    [Test]
    public async Task group_the_same_question_of_the_same_chatbot_ignoring_case_and_spaces_and_count_it()
    {
        Guid bot = Guid.NewGuid();
        this.ShouldFindUnhelpfulAnswers(
        [
            AnswerFeedbackEntryMother.With(bot, "How do I deploy?"),
            AnswerFeedbackEntryMother.With(bot, "  how do i DEPLOY?  "),
            AnswerFeedbackEntryMother.With(bot, "Where are the logs?")
        ]);

        UnhelpfulQuestionsResponse response = await this._handler.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        response.Questions.Should().Equal(
            new UnhelpfulQuestionRow(bot, "How do I deploy?", 2),
            new UnhelpfulQuestionRow(bot, "Where are the logs?", 1));
    }

    [Test]
    public async Task keep_apart_the_same_question_asked_to_different_chatbots()
    {
        Guid first = new("00000000-0000-0000-0000-000000000001");
        Guid second = new("00000000-0000-0000-0000-000000000002");
        this.ShouldFindUnhelpfulAnswers(
        [
            AnswerFeedbackEntryMother.With(second, "Same question"),
            AnswerFeedbackEntryMother.With(first, "Same question")
        ]);

        UnhelpfulQuestionsResponse response = await this._handler.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        response.Questions.Select(row => row.BotKey).Should().Equal(first, second);
    }

    [Test]
    public async Task return_at_most_the_configured_number_of_questions_from_the_most_repeated()
    {
        UnhelpfulQuestionsFinder.Handler limited = new(new(
            this.FeedbackRepository.Object,
            this.Calendar,
            Microsoft.Extensions.Options.Options.Create(Test.Modules.Shared.AnswerFeedbackOptionsMother.With(unhelpfulQuestionsLimit: 2))));
        Guid bot = Guid.NewGuid();
        this.ShouldFindUnhelpfulAnswers(
        [
            AnswerFeedbackEntryMother.With(bot, "a"),
            AnswerFeedbackEntryMother.With(bot, "b"),
            AnswerFeedbackEntryMother.With(bot, "b"),
            AnswerFeedbackEntryMother.With(bot, "c"),
            AnswerFeedbackEntryMother.With(bot, "c"),
            AnswerFeedbackEntryMother.With(bot, "c")
        ]);

        UnhelpfulQuestionsResponse response = await limited.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        response.Questions.Select(row => row.Question).Should().Equal("c", "b");
    }

    [Test]
    public async Task look_from_monday_of_the_current_week_to_the_end_of_today_in_the_configured_time_zone()
    {
        await this._handler.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        UnhelpfulAnswersBetweenSpecification search = this.Searches.Should().ContainSingle().Subject;
        search.FromInclusive.Should().Be(new DateTime(2026, 10, 18, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 22, 22, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task start_the_week_on_monday_even_when_today_is_sunday()
    {
        this.ShouldBeNow(new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc));

        await this._handler.Handle(new UnhelpfulQuestionsFinder.Query(), CancellationToken.None);

        this.Searches.Should().ContainSingle().Which.FromInclusive
            .Should().Be(new DateTime(2026, 10, 18, 22, 0, 0, DateTimeKind.Utc));
    }
}
