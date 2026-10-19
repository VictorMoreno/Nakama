using Nakama.FormationDetail.Application.Handlers;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailCreatedOnFormationCreatedShould : FormationDetailUnitTestCase
{
    private FormationDetailCreatedOnFormationCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailCreatedOnFormationCreated(this.DetailRepository.Object);
    }

    [Test]
    public async Task save_a_detail_with_the_data_of_the_formation()
    {
        FormationCreated @event = FormationCreatedMother.Random();

        await this._handler.Handle(new DomainEventNotification<FormationCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFormationDetailMatching(detail =>
            detail.Id == @event.Id.Value
            && detail.Title == @event.Title
            && detail.Audience == @event.Audience
            && detail.Description == @event.Description
            && detail.Status == @event.Status.ToString()
            && !detail.Archived
            && detail.IconUrl == null
            && detail.Modules.Count == 0
            && detail.Team.Count == 0);
        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
