using Nakama.FormationEnrollment.Application.TeamMemberRemover;
using Nakama.FormationEnrollment.Domain.Events;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationTeamMemberRemoverShould : FormationEnrollmentUnitTestCase
{
    private FormationTeamMemberRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationTeamMemberRemover.Handler(
            new(this.EnrollmentsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_enrollment_and_publish_person_unenrolled()
    {
        Enrollment enrollment = FormationEnrollmentMother.Random();
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationTeamMemberRemoverCommandMother.With(enrollment.FormationId, enrollment.UserId),
            CancellationToken.None);

        this.ShouldHaveRemovedEnrollment(enrollment);
        this.ShouldHavePublished<PersonUnenrolled>(
            @event => @event.FormationId == enrollment.FormationId && @event.UserId == enrollment.UserId);
    }

    [Test]
    public async Task do_nothing_when_the_user_is_not_enrolled()
    {
        this.ShouldNotFindEnrollment();

        await this._handler.Handle(FormationTeamMemberRemoverCommandMother.Random(), CancellationToken.None);

        this.ShouldNotHaveRemovedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }
}
