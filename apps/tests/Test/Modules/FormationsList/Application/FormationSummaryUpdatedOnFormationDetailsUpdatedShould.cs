using AwesomeAssertions;
using Nakama.FormationsList.Application.Handlers;
using Nakama.FormationsList.Domain;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationsList.Domain;
using Test.Modules.Formations.Domain;

namespace Test.Modules.FormationsList.Application;

[TestFixture]
public sealed class FormationSummaryUpdatedOnFormationDetailsUpdatedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationDetailsUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationDetailsUpdated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task replace_the_title_the_audience_and_the_description_of_the_summary()
    {
        FormationSummary summary = FormationSummaryMother.Random();
        string title = $"{summary.Title} updated";
        string audience = $"{summary.Audience} updated";
        string description = $"{summary.Description} updated";
        FormationDetailsUpdated @event = FormationDetailsUpdatedMother.With(FormationId.From(summary.Id), title, audience, description);
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationDetailsUpdated>(@event), CancellationToken.None);

        summary.Title.Should().Be(title);
        summary.Audience.Should().Be(audience);
        summary.Description.Should().Be(description);
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationDetailsUpdated @event = FormationDetailsUpdatedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationDetailsUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
