using AwesomeAssertions;
using Nakama.Formations.Application.Archiver;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationArchiverShould : FormationsUnitTestCase
{
    private FormationArchiver.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationArchiver.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task archive_the_formation_and_publish_formation_archived()
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationArchiverCommandMother.With(formation.Id.Value), CancellationToken.None);

        formation.Archived.Should().BeTrue();
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationArchived>(@event => @event.Id == formation.Id);
    }

    [Test]
    public async Task record_no_new_event_when_the_formation_was_already_archived()
    {
        Formation formation = FormationMother.Archived();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationArchiverCommandMother.With(formation.Id.Value), CancellationToken.None);

        formation.Archived.Should().BeTrue();
        formation.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldFindFormation(null);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationArchiverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
