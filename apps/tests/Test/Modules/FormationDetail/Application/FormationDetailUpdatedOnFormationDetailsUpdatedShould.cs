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
public sealed class FormationDetailUpdatedOnFormationDetailsUpdatedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationDetailsUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationDetailsUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_the_title_the_audience_and_the_description_of_the_detail()
    {
        FormationDetailEntity detail = FormationDetailMother.Random();
        string title = $"{detail.Title} updated";
        string audience = $"{detail.Audience} updated";
        string description = $"{detail.Description} updated";
        FormationDetailsUpdated @event = FormationDetailsUpdatedMother.With(FormationId.From(detail.Id), title, audience, description);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationDetailsUpdated>(@event), CancellationToken.None);

        detail.Title.Should().Be(title);
        detail.Audience.Should().Be(audience);
        detail.Description.Should().Be(description);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationDetailsUpdated @event = FormationDetailsUpdatedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationDetailsUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
