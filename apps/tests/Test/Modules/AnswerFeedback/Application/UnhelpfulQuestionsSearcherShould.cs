using AwesomeAssertions;
using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Searcher;
using Nakama.AnswerFeedback.Domain;
using Test.Modules.AnswerFeedback.Domain;

namespace Test.Modules.AnswerFeedback.Application;

[TestFixture]
public sealed class UnhelpfulQuestionsSearcherShould : AnswerFeedbackUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private UnhelpfulQuestionsSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UnhelpfulQuestionsSearcher.Handler(
            new(this.FeedbackRepository.Object, this.Calendar, this.FeedbackOptions));
        this.ShouldBeNow(Now);
        this.ShouldFindUnhelpfulAnswers([]);
    }

    [Test]
    public async Task return_nothing_when_no_answer_of_the_chatbot_was_rated_as_not_useful()
    {
        UnhelpfulQuestionsResponse response = await this._handler.Handle(
            new UnhelpfulQuestionsSearcher.Query(Guid.NewGuid()), CancellationToken.None);

        response.Questions.Should().BeEmpty();
    }

    [Test]
    public async Task group_the_same_question_ignoring_case_and_spaces_and_count_it()
    {
        Guid bot = Guid.NewGuid();
        this.ShouldFindUnhelpfulAnswers(
        [
            AnswerFeedbackEntryMother.With(bot, "How do I deploy?"),
            AnswerFeedbackEntryMother.With(bot, "  how do i DEPLOY?  "),
            AnswerFeedbackEntryMother.With(bot, "Where are the logs?")
        ]);

        UnhelpfulQuestionsResponse response = await this._handler.Handle(
            new UnhelpfulQuestionsSearcher.Query(bot), CancellationToken.None);

        response.Questions.Should().Equal(
            new UnhelpfulQuestionRow(bot, "How do I deploy?", 2),
            new UnhelpfulQuestionRow(bot, "Where are the logs?", 1));
    }

    [Test]
    public async Task return_at_most_the_configured_number_of_questions_from_the_most_repeated()
    {
        UnhelpfulQuestionsSearcher.Handler limited = new(new(
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

        UnhelpfulQuestionsResponse response = await limited.Handle(
            new UnhelpfulQuestionsSearcher.Query(bot), CancellationToken.None);

        response.Questions.Select(row => row.Question).Should().Equal("c", "b");
    }

    [Test]
    public async Task search_only_the_given_chatbot_from_monday_of_the_current_week_to_the_end_of_today()
    {
        Guid bot = Guid.NewGuid();

        await this._handler.Handle(new UnhelpfulQuestionsSearcher.Query(bot), CancellationToken.None);

        UnhelpfulAnswersOfChatbotBetweenSpecification search = this.ChatbotSearches.Should().ContainSingle().Subject;
        search.BotKey.Should().Be(bot);
        search.FromInclusive.Should().Be(new DateTime(2026, 10, 18, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 22, 22, 0, 0, DateTimeKind.Utc));
    }
}
