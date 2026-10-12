using AwesomeAssertions;
using Nakama.Formations.Application.StatusUpdater;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationStatusUpdaterShould : FormationsUnitTestCase
{
    private FormationStatusUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationStatusUpdater.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task change_the_status_and_publish_formation_status_changed()
    {
        Formation formation = FormationMother.With(status: FormationStatus.Draft);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationStatusUpdaterCommandMother.With(formation.Id.Value, FormationStatus.Active),
            CancellationToken.None);

        formation.Status.Should().Be(FormationStatus.Active);
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationStatusChanged>(
            @event => @event.Id == formation.Id && @event.Status == FormationStatus.Active);
    }

    [Test]
    public async Task record_no_new_event_when_the_status_is_the_same()
    {
        Formation formation = FormationMother.With(status: FormationStatus.Active);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationStatusUpdaterCommandMother.With(formation.Id.Value, FormationStatus.Active),
            CancellationToken.None);

        formation.Status.Should().Be(FormationStatus.Active);
        formation.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldFindFormation(null);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationStatusUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
