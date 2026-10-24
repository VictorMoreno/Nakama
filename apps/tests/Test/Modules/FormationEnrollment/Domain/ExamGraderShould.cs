using AwesomeAssertions;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;

namespace Test.Modules.FormationEnrollment.Domain;

[TestFixture]
public sealed class ExamGraderShould
{
    private static readonly ExamAnswerKey[] Key = [new(3, 0), new(3, 1), new(3, 2)];

    [TestCase(new[] { 0, 1, 2 }, 100)]
    [TestCase(new[] { 0, 1, 0 }, 67)]
    [TestCase(new[] { 0, 0, 0 }, 33)]
    [TestCase(new[] { 2, 2, 1 }, 0)]
    public void score_the_percentage_of_correct_answers_rounded_to_the_nearest_integer(int[] answers, int expected)
    {
        ExamGrader.Score(answers, Key).Should().Be(expected);
    }

    [Test]
    public void round_half_up()
    {
        ExamAnswerKey[] key = Enumerable.Range(0, 8).Select(_ => new ExamAnswerKey(2, 0)).ToArray();

        ExamGrader.Score([0, 1, 1, 1, 1, 1, 1, 1], key).Should().Be(13);
    }

    [TestCase(new[] { 0, 1 })]
    [TestCase(new[] { 0, 1, 2, 0 })]
    [TestCase(new int[0])]
    public void reject_answers_that_do_not_match_the_number_of_questions(int[] answers)
    {
        Action act = () => ExamGrader.Score(answers, Key);

        act.Should().Throw<FormationEnrollmentInvalidExamAnswersError>();
    }

    [TestCase(new[] { 0, 1, 3 })]
    [TestCase(new[] { -1, 1, 2 })]
    public void reject_answers_that_are_not_one_of_the_options(int[] answers)
    {
        Action act = () => ExamGrader.Score(answers, Key);

        act.Should().Throw<FormationEnrollmentInvalidExamAnswersError>();
    }

    [Test]
    public void reject_an_exam_without_questions()
    {
        Action act = () => ExamGrader.Score([], []);

        act.Should().Throw<FormationEnrollmentInvalidExamAnswersError>();
    }
}
