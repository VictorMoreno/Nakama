using AwesomeAssertions;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Finder;
using Nakama.FormationEnrollment.Domain.Errors;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationEnrollmentFinderShould : FormationEnrollmentUnitTestCase
{
    private FormationEnrollmentFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationEnrollmentFinder.Handler(new(this.EnrollmentsRepository.Object));
    }

    [Test]
    public async Task return_the_progress_of_the_enrollment()
    {
        Guid completed = Guid.NewGuid();
        Guid other = Guid.NewGuid();
        Dictionary<Guid, int> scores = new() { [completed] = 70 };
        Enrollment enrollment = FormationEnrollmentMother.With(
            progress: LearnerProgressMother.With(completedModuleIds: [completed, other], examScores: scores));
        this.ShouldFindEnrollment(enrollment);

        FormationEnrollmentResponse response = await this._handler.Handle(
            FormationEnrollmentFinderQueryMother.With(enrollment.FormationId, enrollment.UserId),
            CancellationToken.None);

        response.FormationId.Should().Be(enrollment.FormationId);
        response.UserId.Should().Be(enrollment.UserId);
        response.CompletedModuleIds.Should().Equal(completed, other);
        response.ExamScores.Should().ContainKey(completed).WhoseValue.Should().Be(70);
    }

    [Test]
    public async Task return_an_empty_progress_when_nothing_has_been_completed()
    {
        Enrollment enrollment = FormationEnrollmentMother.Random();
        this.ShouldFindEnrollment(enrollment);

        FormationEnrollmentResponse response = await this._handler.Handle(
            FormationEnrollmentFinderQueryMother.With(enrollment.FormationId, enrollment.UserId),
            CancellationToken.None);

        response.CompletedModuleIds.Should().BeEmpty();
        response.ExamScores.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_the_user_is_not_enrolled()
    {
        this.ShouldNotFindEnrollment();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationEnrollmentFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentNotFoundError>();
    }
}
