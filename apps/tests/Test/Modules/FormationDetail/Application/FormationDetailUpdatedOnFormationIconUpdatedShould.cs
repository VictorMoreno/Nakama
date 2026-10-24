using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationIconUpdatedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationIconUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationIconUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task set_the_icon_of_the_detail()
    {
        FormationDetailEntity detail = FormationDetailMother.With(iconUrl: null);
        FormationIconUpdated @event = FormationIconUpdatedMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        detail.IconUrl.Should().Be(@event.IconUrl);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task clear_the_icon_of_the_detail_when_the_event_has_none()
    {
        FormationDetailEntity detail = FormationDetailMother.With(iconUrl: "icon.png");
        FormationIconUpdated @event = FormationIconUpdatedMother.Cleared(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        detail.IconUrl.Should().BeNull();
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationIconUpdated @event = FormationIconUpdatedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
