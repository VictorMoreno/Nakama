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
public sealed class FormationSummaryUpdatedOnFormationUnarchivedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationUnarchived _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationUnarchived(this.SummariesRepository.Object);
    }

    [Test]
    public async Task mark_the_summary_as_unarchived()
    {
        FormationSummary summary = FormationSummaryMother.With(archived: true);
        FormationUnarchived @event = FormationUnarchivedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationUnarchived>(@event), CancellationToken.None);

        summary.Archived.Should().BeFalse();
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationUnarchived @event = FormationUnarchivedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationUnarchived>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
