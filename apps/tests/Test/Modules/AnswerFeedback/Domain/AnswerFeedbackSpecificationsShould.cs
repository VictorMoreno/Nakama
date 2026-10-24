using AwesomeAssertions;
using Nakama.AnswerFeedback.Domain;

namespace Test.Modules.AnswerFeedback.Domain;

[TestFixture]
public sealed class AnswerFeedbackSpecificationsShould
{
    private static readonly DateTime From = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void match_only_the_not_useful_answers_of_the_given_chatbot_rated_inside_the_range()
    {
        Guid bot = Guid.NewGuid();
        UnhelpfulAnswersOfChatbotBetweenSpecification specification = new(bot, From, From.AddDays(1));

        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(botKey: bot, useful: false, ratedAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false, ratedAt: From)).Should().BeFalse();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(botKey: bot, useful: true, ratedAt: From)).Should().BeFalse();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(botKey: bot, useful: false, ratedAt: From.AddDays(1))).Should().BeFalse();
    }

    [Test]
    public void match_only_the_not_useful_answers_rated_inside_the_range_including_the_start_and_excluding_the_end()
    {
        UnhelpfulAnswersBetweenSpecification specification = new(From, From.AddDays(1));

        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false, ratedAt: From)).Should().BeTrue();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddDays(1))).Should().BeFalse();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false, ratedAt: From.AddTicks(-1))).Should().BeFalse();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: true, ratedAt: From)).Should().BeFalse();
    }

    [Test]
    public void tell_the_useful_answers_apart_from_the_rest()
    {
        UsefulAnswersSpecification specification = new();

        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: true)).Should().BeTrue();
        specification.IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false)).Should().BeFalse();
    }

    [Test]
    public void match_every_rated_answer()
    {
        new RatedAnswersSpecification().IsSatisfiedBy(AnswerFeedbackEntryMother.With(useful: false)).Should().BeTrue();
    }
}
