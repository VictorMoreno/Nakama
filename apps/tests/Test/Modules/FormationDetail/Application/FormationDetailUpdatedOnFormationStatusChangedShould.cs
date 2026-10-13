using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationStatusChangedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationStatusChanged _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationStatusChanged(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_the_status_of_the_detail()
    {
        FormationDetailEntity detail = FormationDetailMother.With(status: FormationStatus.Active.ToString());
        FormationStatusChanged @event = FormationStatusChangedMother.With(FormationId.From(detail.Id), FormationStatus.Inactive);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationStatusChanged>(@event), CancellationToken.None);

        detail.Status.Should().Be(FormationStatus.Inactive.ToString());
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationStatusChanged @event = FormationStatusChangedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationStatusChanged>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
