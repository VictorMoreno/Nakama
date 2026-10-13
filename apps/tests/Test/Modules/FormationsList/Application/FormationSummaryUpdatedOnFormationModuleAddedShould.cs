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
public sealed class FormationSummaryUpdatedOnFormationModuleAddedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationModuleAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationModuleAdded(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_more_module()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 2);
        FormationModuleAdded @event = FormationModuleAddedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        summary.ModulesCount.Should().Be(3);
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationModuleAdded @event = FormationModuleAddedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
