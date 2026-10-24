using AwesomeAssertions;
using Nakama.AnswerFeedback.Domain;
using Nakama.AnswerFeedback.Infrastructure;
using Test.Modules.AnswerFeedback.Domain;
using Test.Modules.Shared.Infrastructure;

namespace Test.Modules.AnswerFeedback.Infrastructure;

[TestFixture]
public sealed class NhAnswerFeedbackRepositoryShould : InfrastructureTestCase
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private NhAnswerFeedbackRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhAnswerFeedbackRepository(this.UnitOfWork);
    }

    [Test]
    public async Task keep_every_field_of_a_saved_rating()
    {
        AnswerFeedbackEntry entry = AnswerFeedbackEntryMother.With(question: "How do I deploy?", useful: false, ratedAt: From.AddHours(3));
        await this._repository.Save(entry, CancellationToken.None);
        await this.Persist();

        AnswerFeedbackEntry? found = await this._repository.FindById(entry.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.BotKey.Should().Be(entry.BotKey);
        found.Question.Should().Be("How do I deploy?");
        found.Useful.Should().BeFalse();
        found.RatedAt.Should().BeCloseTo(entry.RatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public async Task store_a_long_question_without_truncating_it()
    {
        string question = new('q', 5000);
        AnswerFeedbackEntry entry = AnswerFeedbackEntryMother.With(question: question);
        await this._repository.Save(entry, CancellationToken.None);
        await this.Persist();

        AnswerFeedbackEntry? found = await this._repository.FindById(entry.Id, CancellationToken.None);

        found!.Question.Should().Be(question);
    }

    [Test]
    public async Task change_the_rating_of_an_entry_that_already_exists()
    {
        AnswerFeedbackEntry entry = AnswerFeedbackEntryMother.With(useful: false, ratedAt: From);
        await this._repository.Save(entry, CancellationToken.None);
        await this.Persist();
        AnswerFeedbackEntry? stored = await this._repository.FindById(entry.Id, CancellationToken.None);
        stored!.Rate(true, From.AddMinutes(5));

        await this._repository.Save(stored, CancellationToken.None);
        await this.Persist();

        AnswerFeedbackEntry? found = await this._repository.FindById(entry.Id, CancellationToken.None);
        found!.Useful.Should().BeTrue();
        (await this._repository.Count(new RatedAnswersSpecification(), CancellationToken.None)).Should().Be(1);
    }

    [Test]
    public async Task count_the_useful_and_the_rated_answers()
    {
        await this._repository.Save(AnswerFeedbackEntryMother.With(useful: true), CancellationToken.None);
        await this._repository.Save(AnswerFeedbackEntryMother.With(useful: true), CancellationToken.None);
        await this._repository.Save(AnswerFeedbackEntryMother.With(useful: false), CancellationToken.None);
        await this.Persist();

        (await this._repository.Count(new UsefulAnswersSpecification(), CancellationToken.None)).Should().Be(2);
        (await this._repository.Count(new RatedAnswersSpecification(), CancellationToken.None)).Should().Be(3);
    }

    [Test]
    public async Task find_only_the_not_useful_answers_of_the_given_chatbot()
    {
        Guid bot = Guid.NewGuid();
        AnswerFeedbackEntry mine = AnswerFeedbackEntryMother.With(botKey: bot, useful: false, ratedAt: From.AddHours(1));
        foreach (AnswerFeedbackEntry entry in new[]
        {
            mine,
            AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddHours(2)),
            AnswerFeedbackEntryMother.With(botKey: bot, useful: true, ratedAt: From.AddHours(3))
        })
        {
            await this._repository.Save(entry, CancellationToken.None);
        }

        await this.Persist();

        IReadOnlyList<AnswerFeedbackEntry> found = await this._repository.Search(
            new UnhelpfulAnswersOfChatbotBetweenSpecification(bot, From, From.AddDays(1)),
            CancellationToken.None);

        found.Select(entry => entry.Id).Should().Equal(mine.Id);
    }

    [Test]
    public async Task find_the_not_useful_answers_inside_the_range_ordered_by_date()
    {
        AnswerFeedbackEntry second = AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddHours(10));
        AnswerFeedbackEntry first = AnswerFeedbackEntryMother.With(useful: false, ratedAt: From);
        foreach (AnswerFeedbackEntry entry in new[]
        {
            second,
            first,
            AnswerFeedbackEntryMother.With(useful: true, ratedAt: From.AddHours(1)),
            AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddDays(1)),
            AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddSeconds(-1))
        })
        {
            await this._repository.Save(entry, CancellationToken.None);
        }

        await this.Persist();

        IReadOnlyList<AnswerFeedbackEntry> found = await this._repository.Search(
            new UnhelpfulAnswersBetweenSpecification(From, From.AddDays(1)),
            CancellationToken.None);

        found.Select(entry => entry.Id).Should().Equal(first.Id, second.Id);
    }

    [Test]
    public async Task have_the_index_the_range_query_relies_on()
    {
        object? indexes = await this.UnitOfWork.Session
            .CreateSQLQuery(
                "select count(*) from pg_indexes where tablename = 'read_answer_feedback' " +
                "and indexname = 'ix_read_answer_feedback_ratedat'")
            .UniqueResultAsync();

        Convert.ToInt32(indexes).Should().Be(1);
    }
}
