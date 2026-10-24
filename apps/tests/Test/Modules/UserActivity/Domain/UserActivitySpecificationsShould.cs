using AwesomeAssertions;
using Nakama.UserActivity.Domain;

namespace Test.Modules.UserActivity.Domain;

[TestFixture]
public sealed class UserActivitySpecificationsShould
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void include_the_start_and_exclude_the_end_of_the_range()
    {
        UserActivityBetweenSpecification specification = new(From, From.AddDays(1));

        specification.IsSatisfiedBy(UserActivityEntryMother.With(occurredAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(occurredAt: From.AddDays(1).AddTicks(-1))).Should().BeTrue();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(occurredAt: From.AddDays(1))).Should().BeFalse();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(occurredAt: From.AddTicks(-1))).Should().BeFalse();
    }

    [Test]
    public void match_only_the_activity_of_that_person_since_the_given_moment()
    {
        Guid userId = Guid.NewGuid();
        UserActivityOfUserSinceSpecification specification = new(userId, From);

        specification.IsSatisfiedBy(UserActivityEntryMother.With(userId, occurredAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(userId, occurredAt: From.AddTicks(-1))).Should().BeFalse();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(Guid.NewGuid(), occurredAt: From.AddDays(1))).Should().BeFalse();
    }

    [Test]
    public void match_only_the_activity_of_the_given_person_in_the_range_when_one_is_set()
    {
        Guid userId = Guid.NewGuid();
        UserActivityBetweenSpecification specification = new(From, From.AddDays(1), userId);

        specification.IsSatisfiedBy(UserActivityEntryMother.With(userId, occurredAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(Guid.NewGuid(), occurredAt: From)).Should().BeFalse();
        specification.IsSatisfiedBy(UserActivityEntryMother.With(userId, occurredAt: From.AddDays(1))).Should().BeFalse();
    }
}
