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
public sealed class FormationSummaryUpdatedOnFormationArchivedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationArchived _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationArchived(this.SummariesRepository.Object);
    }

    [Test]
    public async Task mark_the_summary_as_archived()
    {
        FormationSummary summary = FormationSummaryMother.With(archived: false);
        FormationArchived @event = FormationArchivedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationArchived>(@event), CancellationToken.None);

        summary.Archived.Should().BeTrue();
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationArchived @event = FormationArchivedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationArchived>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
