using AwesomeAssertions;
using Nakama.Formations.Application.ModuleGatingToggler;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationModuleGatingTogglerShould : FormationsUnitTestCase
{
    private FormationModuleGatingToggler.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationModuleGatingToggler.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task turn_gating_on_when_the_module_was_not_gated()
    {
        FormationModule module = FormationModuleMother.With(gated: false);
        Formation formation = FormationMother.With(modules: [module]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationModuleGatingTogglerCommandMother.With(formation.Id.Value, module.Id.Value),
            CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Gated.Should().BeTrue();
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationModuleGatingToggled>(
            @event => @event.Id == formation.Id && @event.ModuleId == module.Id && @event.Gated);
    }

    [Test]
    public async Task turn_gating_off_when_the_module_was_gated()
    {
        FormationModule module = FormationModuleMother.With(gated: true);
        Formation formation = FormationMother.With(modules: [module]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationModuleGatingTogglerCommandMother.With(formation.Id.Value, module.Id.Value), CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Gated.Should().BeFalse();
        this.ShouldHavePublished<FormationModuleGatingToggled>(
            @event => @event.ModuleId == module.Id && !@event.Gated);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_does_not_exist()
    {
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleGatingTogglerCommandMother.With(formation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationModuleGatingTogglerCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
