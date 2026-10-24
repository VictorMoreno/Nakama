using AwesomeAssertions;
using Nakama.Formations.Application.Unarchiver;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationUnarchiverShould : FormationsUnitTestCase
{
    private FormationUnarchiver.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationUnarchiver.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task unarchive_the_formation_and_publish_formation_unarchived()
    {
        Formation formation = FormationMother.Archived();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationUnarchiverCommandMother.With(formation.Id.Value), CancellationToken.None);

        formation.Archived.Should().BeFalse();
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationUnarchived>(@event => @event.Id == formation.Id);
    }

    [Test]
    public async Task record_no_new_event_when_the_formation_was_not_archived()
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationUnarchiverCommandMother.With(formation.Id.Value), CancellationToken.None);

        formation.Archived.Should().BeFalse();
        formation.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationUnarchiverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
