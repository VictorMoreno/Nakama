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
public sealed class FormationSummaryUpdatedOnFormationStatusChangedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationStatusChanged _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationStatusChanged(this.SummariesRepository.Object);
    }

    [Test]
    public async Task replace_the_status_of_the_summary()
    {
        FormationSummary summary = FormationSummaryMother.With(status: FormationStatus.Active.ToString());
        FormationStatusChanged @event = FormationStatusChangedMother.With(FormationId.From(summary.Id), FormationStatus.Inactive);
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationStatusChanged>(@event), CancellationToken.None);

        summary.Status.Should().Be(FormationStatus.Inactive.ToString());
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationStatusChanged @event = FormationStatusChangedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationStatusChanged>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
