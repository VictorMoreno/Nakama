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
public sealed class FormationSummaryUpdatedOnFormationIconUpdatedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationIconUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationIconUpdated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task set_the_icon_of_the_summary()
    {
        FormationSummary summary = FormationSummaryMother.With(iconUrl: null);
        FormationIconUpdated @event = FormationIconUpdatedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        summary.IconUrl.Should().Be(@event.IconUrl);
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task clear_the_icon_of_the_summary_when_the_event_has_none()
    {
        FormationSummary summary = FormationSummaryMother.With(iconUrl: "icon.png");
        FormationIconUpdated @event = FormationIconUpdatedMother.Cleared(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        summary.IconUrl.Should().BeNull();
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationIconUpdated @event = FormationIconUpdatedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationIconUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
