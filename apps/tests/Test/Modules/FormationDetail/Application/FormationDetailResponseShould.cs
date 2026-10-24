using AwesomeAssertions;
using Nakama.FormationDetail.Application;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailResponseShould
{
    [Test]
    public void hide_the_correct_answer_of_every_exam_question_without_touching_the_rest()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            modules: [FormationModuleResponseMother.Exam(70, 0, 2), FormationModuleResponseMother.Random()]);

        FormationDetailResponse masked = detail.WithoutAnswers();

        FormationModuleResponse exam = masked.Modules[0];
        exam.Quiz.Should().HaveCount(2);
        exam.Quiz.Select(question => question.CorrectIndex).Should().OnlyContain(correctIndex => correctIndex == null);
        exam.Quiz.Select(question => question.Question).Should().Equal(detail.Modules[0].Quiz!.Select(question => question.Question));
        exam.Quiz.Select(question => question.Options).Should().Equal(detail.Modules[0].Quiz!.Select(question => question.Options));
        exam.PassingScore.Should().Be(70);
        masked.Modules[1].Should().Be(detail.Modules[1]);
    }

    [Test]
    public void keep_the_original_response_with_its_answers()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 1);

        _ = detail.WithoutAnswers();

        detail.Modules.Single().Quiz!.Single().CorrectIndex.Should().Be(1);
    }
}
