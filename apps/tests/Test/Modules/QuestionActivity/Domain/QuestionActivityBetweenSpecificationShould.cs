using AwesomeAssertions;
using Nakama.QuestionActivity.Domain;

namespace Test.Modules.QuestionActivity.Domain;

[TestFixture]
public sealed class QuestionActivityBetweenSpecificationShould
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void include_the_start_and_exclude_the_end_of_the_range()
    {
        QuestionActivityBetweenSpecification specification = new(From, From.AddDays(1));

        specification.IsSatisfiedBy(QuestionActivityEntryMother.With(occurredAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(QuestionActivityEntryMother.With(occurredAt: From.AddDays(1).AddTicks(-1))).Should().BeTrue();
        specification.IsSatisfiedBy(QuestionActivityEntryMother.With(occurredAt: From.AddDays(1))).Should().BeFalse();
        specification.IsSatisfiedBy(QuestionActivityEntryMother.With(occurredAt: From.AddTicks(-1))).Should().BeFalse();
    }
}
