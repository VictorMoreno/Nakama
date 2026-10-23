using AwesomeAssertions;
using Nakama.QuestionActivity.Domain;
using Nakama.QuestionActivity.Infrastructure;
using Test.Modules.QuestionActivity.Domain;
using Test.Modules.Shared.Infrastructure;

namespace Test.Modules.QuestionActivity.Infrastructure;

[TestFixture]
public sealed class NhQuestionActivityRepositoryShould : InfrastructureTestCase
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private NhQuestionActivityRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhQuestionActivityRepository(this.UnitOfWork);
    }

    [Test]
    public async Task find_the_questions_inside_the_range_ordered_by_date_including_the_start_and_excluding_the_end()
    {
        QuestionActivityEntry atTheEnd = QuestionActivityEntryMother.With(occurredAt: From.AddDays(1));
        QuestionActivityEntry second = QuestionActivityEntryMother.With(occurredAt: From.AddHours(10));
        QuestionActivityEntry first = QuestionActivityEntryMother.With(occurredAt: From);
        QuestionActivityEntry before = QuestionActivityEntryMother.With(occurredAt: From.AddSeconds(-1));
        foreach (QuestionActivityEntry entry in new[] { atTheEnd, second, first, before })
        {
            await this._repository.Save(entry, CancellationToken.None);
        }

        await this.Persist();

        IReadOnlyList<QuestionActivityEntry> found = await this._repository.Search(
            new QuestionActivityBetweenSpecification(From, From.AddDays(1)),
            CancellationToken.None);

        found.Select(entry => entry.Id).Should().Equal(first.Id, second.Id);
    }

    [Test]
    public async Task keep_every_field_of_a_saved_entry()
    {
        QuestionActivityEntry entry = QuestionActivityEntryMother.With(occurredAt: From.AddHours(3));
        await this._repository.Save(entry, CancellationToken.None);
        await this.Persist();

        QuestionActivityEntry? found = await this._repository.FindById(entry.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.BotKey.Should().Be(entry.BotKey);
        found.UserId.Should().Be(entry.UserId);
        found.OccurredAt.Should().BeCloseTo(entry.OccurredAt, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public async Task have_the_index_the_range_query_relies_on()
    {
        object? indexes = await this.UnitOfWork.Session
            .CreateSQLQuery(
                "select count(*) from pg_indexes where tablename = 'read_question_activity' " +
                "and indexname = 'ix_read_question_activity_occurredat'")
            .UniqueResultAsync();

        Convert.ToInt32(indexes).Should().Be(1);
    }
}
