using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationModuleGatingToggledShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationModuleGatingToggled _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationModuleGatingToggled(this.DetailRepository.Object);
    }

    [Test]
    public async Task set_the_gating_of_the_matching_module_row_only()
    {
        FormationModuleRow target = FormationModuleRowMother.With(gated: false);
        FormationModuleRow other = FormationModuleRowMother.With(gated: false);
        FormationDetailEntity detail = FormationDetailMother.With(modules: [target, other]);
        FormationModuleGatingToggled @event = FormationModuleGatingToggledMother.With(
            FormationId.From(detail.Id),
            ModuleId.From(target.Id),
            true);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleGatingToggled>(@event), CancellationToken.None);

        target.Gated.Should().BeTrue();
        other.Gated.Should().BeFalse();
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task remove_the_gating_when_the_event_says_so()
    {
        FormationModuleRow target = FormationModuleRowMother.With(gated: true);
        FormationDetailEntity detail = FormationDetailMother.With(modules: [target]);
        FormationModuleGatingToggled @event = FormationModuleGatingToggledMother.With(
            FormationId.From(detail.Id),
            ModuleId.From(target.Id),
            false);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleGatingToggled>(@event), CancellationToken.None);

        target.Gated.Should().BeFalse();
    }

    [Test]
    public async Task leave_the_modules_untouched_when_the_module_is_not_in_the_detail()
    {
        FormationModuleRow row = FormationModuleRowMother.With(gated: false);
        FormationDetailEntity detail = FormationDetailMother.With(modules: [row]);
        FormationModuleGatingToggled @event = FormationModuleGatingToggledMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleGatingToggled>(@event), CancellationToken.None);

        row.Gated.Should().BeFalse();
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationModuleGatingToggled @event = FormationModuleGatingToggledMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationModuleGatingToggled>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
