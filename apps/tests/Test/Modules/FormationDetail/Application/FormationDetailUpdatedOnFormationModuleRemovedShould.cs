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
public sealed class FormationDetailUpdatedOnFormationModuleRemovedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationModuleRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationModuleRemoved(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_module_row_with_that_id()
    {
        FormationModuleRow removed = FormationModuleRowMother.Random();
        FormationModuleRow kept = FormationModuleRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(modules: [removed, kept]);
        FormationModuleRemoved @event = FormationModuleRemovedMother.With(
            FormationId.From(detail.Id),
            ModuleId.From(removed.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        detail.Modules.Should().ContainSingle().Which.Should().BeSameAs(kept);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task leave_the_modules_untouched_when_the_module_is_not_in_the_detail()
    {
        FormationModuleRow row = FormationModuleRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(modules: [row]);
        FormationModuleRemoved @event = FormationModuleRemovedMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        detail.Modules.Should().ContainSingle().Which.Should().BeSameAs(row);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationModuleRemoved @event = FormationModuleRemovedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationModuleRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
