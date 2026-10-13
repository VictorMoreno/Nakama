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
public sealed class FormationDetailUpdatedOnPersonEnrolledShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnPersonEnrolled _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnPersonEnrolled(this.DetailRepository.Object);
    }

    [Test]
    public async Task add_the_user_to_the_team_with_zero_progress()
    {
        FormationDetailEntity detail = FormationDetailMother.Random();
        PersonEnrolled @event = PersonEnrolledMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<PersonEnrolled>(@event), CancellationToken.None);

        FormationMemberRow member = detail.Team.Should().ContainSingle().Which;
        member.UserId.Should().Be(@event.UserId);
        member.Progress.Should().Be(0);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task not_duplicate_a_user_who_is_already_in_the_team()
    {
        FormationMemberRow member = FormationMemberRowMother.With(progress: 40);
        FormationDetailEntity detail = FormationDetailMother.With(team: [member]);
        PersonEnrolled @event = PersonEnrolledMother.With(formationId: detail.Id, userId: member.UserId);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<PersonEnrolled>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Progress.Should().Be(40);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        PersonEnrolled @event = PersonEnrolledMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<PersonEnrolled>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
