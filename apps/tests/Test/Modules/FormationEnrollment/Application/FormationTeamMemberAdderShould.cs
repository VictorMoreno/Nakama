using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.FormationEnrollment.Application.TeamMemberAdder;
using Nakama.FormationEnrollment.Domain.Events;
using Test.Modules.FormationDetail.Application;
using Test.Modules.FormationEnrollment.Domain;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationTeamMemberAdderShould : FormationEnrollmentUnitTestCase
{
    private FormationTeamMemberAdder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationTeamMemberAdder.Handler(
            new(this.QueryBus.Object, this.EnrollmentsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task enroll_the_user_with_an_empty_progress_and_publish_person_enrolled()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.Random();
        FormationTeamMemberAdder.Command command = FormationTeamMemberAdderCommandMother.With(detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldNotFindEnrollment();

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldHaveAskedForFormationDetail(detail.Id);
        this.ShouldHaveSavedEnrollmentMatching(enrollment =>
            enrollment.FormationId == detail.Id
            && enrollment.UserId == command.UserId
            && enrollment.Progress.CompletedModuleIds.Count == 0);
        this.ShouldHavePublished<PersonEnrolled>(
            @event => @event.FormationId == detail.Id && @event.UserId == command.UserId);
    }

    [Test]
    public async Task do_nothing_when_the_user_is_already_enrolled()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.Random();
        FormationTeamMemberAdder.Command command = FormationTeamMemberAdderCommandMother.With(detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(
            FormationEnrollmentMother.With(formationId: detail.Id, userId: command.UserId));

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_enrolling_anyone_when_the_formation_does_not_exist()
    {
        FormationTeamMemberAdder.Command command = FormationTeamMemberAdderCommandMother.Random();
        this.ShouldNotFindFormationDetail(command.Id);

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FormationDetailNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }
}
