using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.FormationEnrollment.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnEnrollmentModuleCompletedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnEnrollmentModuleCompleted _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnEnrollmentModuleCompleted(this.DetailRepository.Object);
    }

    [Test]
    public async Task set_the_progress_of_the_member_as_the_percentage_of_completed_modules()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 0);
        FormationDetailEntity detail = FormationDetailMother.With(
            modules:
            [
                FormationModuleRowMother.Random(),
                FormationModuleRowMother.Random(),
                FormationModuleRowMother.Random(),
                FormationModuleRowMother.Random()
            ],
            team: [member]);
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.With(
            formationId: detail.Id,
            userId: member.UserId,
            completedModuleCount: 1);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        member.Progress.Should().Be(25);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task round_the_progress_down_to_a_whole_percentage()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 0);
        FormationDetailEntity detail = FormationDetailMother.With(
            modules: [FormationModuleRowMother.Random(), FormationModuleRowMother.Random(), FormationModuleRowMother.Random()],
            team: [member]);
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.With(
            formationId: detail.Id,
            userId: member.UserId,
            completedModuleCount: 1);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        member.Progress.Should().Be(33);
    }

    [Test]
    public async Task set_a_zero_progress_when_the_formation_has_no_modules()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 40);
        FormationDetailEntity detail = FormationDetailMother.With(team: [member]);
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.With(
            formationId: detail.Id,
            userId: member.UserId,
            completedModuleCount: 1);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        member.Progress.Should().Be(0);
    }

    [Test]
    public async Task leave_the_other_members_untouched()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 0);
        FormationMemberRow other = FormationMemberRowMother.With(progress: 7);
        FormationDetailEntity detail = FormationDetailMother.With(
            modules: [FormationModuleRowMother.Random(), FormationModuleRowMother.Random()],
            team: [member, other]);
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.With(
            formationId: detail.Id,
            userId: member.UserId,
            completedModuleCount: 2);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        member.Progress.Should().Be(100);
        other.Progress.Should().Be(7);
    }

    [Test]
    public async Task change_no_progress_when_the_user_is_not_a_member()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 15);
        FormationDetailEntity detail = FormationDetailMother.With(
            modules: [FormationModuleRowMother.Random()],
            team: [member]);
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        member.Progress.Should().Be(15);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        EnrollmentModuleCompleted @event = EnrollmentModuleCompletedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<EnrollmentModuleCompleted>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
