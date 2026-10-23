using AwesomeAssertions;
using Nakama.UserActivity.Domain;
using Nakama.UserActivity.Infrastructure;
using Test.Modules.Shared.Infrastructure;
using Test.Modules.UserActivity.Domain;

namespace Test.Modules.UserActivity.Infrastructure;

[TestFixture]
public sealed class NhUserActivityRepositoryShould : InfrastructureTestCase
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private NhUserActivityRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhUserActivityRepository(this.UnitOfWork);
    }

    [Test]
    public async Task find_the_activity_inside_the_range_ordered_by_date_including_the_start_and_excluding_the_end()
    {
        UserActivityEntry atTheEnd = UserActivityEntryMother.With(occurredAt: From.AddDays(1));
        UserActivityEntry second = UserActivityEntryMother.With(occurredAt: From.AddHours(10));
        UserActivityEntry first = UserActivityEntryMother.With(occurredAt: From);
        UserActivityEntry before = UserActivityEntryMother.With(occurredAt: From.AddSeconds(-1));
        foreach (UserActivityEntry entry in new[] { atTheEnd, second, first, before })
        {
            await this._repository.Save(entry, CancellationToken.None);
        }

        await this.Persist();

        IReadOnlyList<UserActivityEntry> found = await this._repository.Search(
            new UserActivityBetweenSpecification(From, From.AddDays(1)),
            CancellationToken.None);

        found.Select(entry => entry.Id).Should().Equal(first.Id, second.Id);
    }

    [Test]
    public async Task keep_every_field_of_a_saved_entry()
    {
        UserActivityEntry entry = UserActivityEntryMother.With(type: UserActivityTypes.Session, occurredAt: From.AddHours(3));
        await this._repository.Save(entry, CancellationToken.None);
        await this.Persist();

        UserActivityEntry? found = await this._repository.FindById(entry.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.UserId.Should().Be(entry.UserId);
        found.Type.Should().Be(UserActivityTypes.Session);
        found.OccurredAt.Should().BeCloseTo(entry.OccurredAt, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public async Task count_only_the_activity_of_that_person_since_the_given_moment()
    {
        Guid userId = Guid.NewGuid();
        await this._repository.Save(UserActivityEntryMother.With(userId, occurredAt: From.AddHours(1)), CancellationToken.None);
        await this._repository.Save(UserActivityEntryMother.With(userId, occurredAt: From), CancellationToken.None);
        await this._repository.Save(UserActivityEntryMother.With(userId, occurredAt: From.AddSeconds(-1)), CancellationToken.None);
        await this._repository.Save(UserActivityEntryMother.With(Guid.NewGuid(), occurredAt: From.AddHours(2)), CancellationToken.None);
        await this.Persist();

        int count = await this._repository.Count(new UserActivityOfUserSinceSpecification(userId, From), CancellationToken.None);

        count.Should().Be(2);
    }

    [Test]
    public async Task have_the_indexes_the_activity_queries_rely_on()
    {
        object? indexes = await this.UnitOfWork.Session
            .CreateSQLQuery(
                "select count(*) from pg_indexes where tablename = 'read_user_activity' " +
                "and indexname in ('ix_read_user_activity_occurredat', 'ix_read_user_activity_userid_occurredat')")
            .UniqueResultAsync();

        Convert.ToInt32(indexes).Should().Be(2);
    }
}
