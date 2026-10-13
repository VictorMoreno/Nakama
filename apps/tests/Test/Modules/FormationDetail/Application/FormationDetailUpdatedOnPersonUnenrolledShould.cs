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
public sealed class FormationDetailUpdatedOnPersonUnenrolledShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnPersonUnenrolled _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnPersonUnenrolled(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_user_from_the_team()
    {
        FormationMemberRow removed = FormationMemberRowMother.Random();
        FormationMemberRow kept = FormationMemberRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(team: [removed, kept]);
        PersonUnenrolled @event = PersonUnenrolledMother.With(formationId: detail.Id, userId: removed.UserId);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<PersonUnenrolled>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(kept);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task leave_the_team_untouched_when_the_user_is_not_a_member()
    {
        FormationMemberRow member = FormationMemberRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(team: [member]);
        PersonUnenrolled @event = PersonUnenrolledMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<PersonUnenrolled>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(member);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        PersonUnenrolled @event = PersonUnenrolledMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<PersonUnenrolled>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
