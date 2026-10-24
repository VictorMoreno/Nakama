using AwesomeAssertions;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Searcher;
using Nakama.FormationEnrollment.Domain;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationEnrollmentSearcherShould : FormationEnrollmentUnitTestCase
{
    private FormationEnrollmentSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationEnrollmentSearcher.Handler(new(this.EnrollmentsRepository.Object));
    }

    [Test]
    public async Task return_the_progress_and_exam_attempts_of_everyone_in_the_formation()
    {
        Guid formationId = Guid.NewGuid();
        Guid module = Guid.NewGuid();
        DateTime attemptedAt = DateTime.UtcNow;
        Enrollment first = FormationEnrollmentMother.With(
            formationId: formationId,
            progress: LearnerProgressMother.With(
                completedModuleIds: [module],
                examScores: new Dictionary<Guid, int> { [module] = 90 },
                examAttempts: [new ExamAttempt(module, 40, false, attemptedAt.AddHours(-1)), new ExamAttempt(module, 90, true, attemptedAt)]));
        Enrollment second = FormationEnrollmentMother.With(formationId: formationId);
        this.ShouldFindEnrollmentsOfFormation(formationId, first, second);

        IReadOnlyCollection<FormationEnrollmentResponse> response = await this._handler.Handle(
            new FormationEnrollmentSearcher.Query(formationId),
            CancellationToken.None);

        response.Select(item => item.UserId).Should().Equal(first.UserId, second.UserId);
        response.First().ExamAttempts.Select(attempt => (attempt.Score, attempt.Passed)).Should().Equal((40, false), (90, true));
        response.First().CompletedModuleIds.Should().Equal(module);
        response.Last().ExamAttempts.Should().BeEmpty();
    }

    [Test]
    public async Task return_an_empty_collection_when_nobody_is_assigned()
    {
        Guid formationId = Guid.NewGuid();
        this.ShouldFindEnrollmentsOfFormation(formationId);

        IReadOnlyCollection<FormationEnrollmentResponse> response = await this._handler.Handle(
            new FormationEnrollmentSearcher.Query(formationId),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
