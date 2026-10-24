using Nakama.FormationsList.Application.Handlers;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.FormationsList.Application;

[TestFixture]
public sealed class FormationSummaryCreatedOnFormationCreatedShould : FormationsListUnitTestCase
{
    private FormationSummaryCreatedOnFormationCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryCreatedOnFormationCreated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task save_a_summary_with_the_data_of_the_formation_and_no_modules()
    {
        FormationCreated @event = FormationCreatedMother.Random();

        await this._handler.Handle(new DomainEventNotification<FormationCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFormationSummaryMatching(summary =>
            summary.Id == @event.Id.Value
            && summary.Title == @event.Title
            && summary.Audience == @event.Audience
            && summary.Description == @event.Description
            && summary.Status == @event.Status.ToString()
            && !summary.Archived
            && summary.IconUrl == null
            && summary.ModulesCount == 0);
        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
