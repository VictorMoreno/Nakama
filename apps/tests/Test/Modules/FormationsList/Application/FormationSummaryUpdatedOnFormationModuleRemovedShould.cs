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
public sealed class FormationSummaryUpdatedOnFormationModuleRemovedShould : FormationsListUnitTestCase
{
    private FormationSummaryUpdatedOnFormationModuleRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryUpdatedOnFormationModuleRemoved(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_module_less()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 3);
        FormationModuleRemoved @event = FormationModuleRemovedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        summary.ModulesCount.Should().Be(2);
        this.ShouldHaveUpdatedFormationSummary(summary);
    }

    [Test]
    public async Task keep_the_module_count_at_zero_when_there_are_no_modules()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 0);
        FormationModuleRemoved @event = FormationModuleRemovedMother.With(FormationId.From(summary.Id));
        this.ShouldFindFormationSummary(summary);

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        summary.ModulesCount.Should().Be(0);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        FormationModuleRemoved @event = FormationModuleRemovedMother.Random();
        this.ShouldNotFindFormationSummary();

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationSummary();
    }
}
