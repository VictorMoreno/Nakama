using AwesomeAssertions;
using Nakama.Formations.Application.ModuleRemover;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationModuleRemoverShould : FormationsUnitTestCase
{
    private FormationModuleRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationModuleRemover.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_module_and_publish_formation_module_removed()
    {
        FormationModule module = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [module]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationModuleRemoverCommandMother.With(formation.Id.Value, module.Id.Value),
            CancellationToken.None);

        formation.Modules.Should().BeEmpty();
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationModuleRemoved>(@event => @event.Id == formation.Id && @event.ModuleId == module.Id);
    }

    [Test]
    public async Task keep_the_other_modules_when_one_is_removed()
    {
        FormationModule kept = FormationModuleMother.Random();
        FormationModule removed = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [removed, kept]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationModuleRemoverCommandMother.With(formation.Id.Value, removed.Id.Value), CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Should().Be(kept);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_does_not_exist()
    {
        FormationModule existing = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [existing]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleRemoverCommandMother.With(formation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleNotFoundError>();
        formation.Modules.Should().ContainSingle().Which.Should().Be(existing);
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationModuleRemoverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
