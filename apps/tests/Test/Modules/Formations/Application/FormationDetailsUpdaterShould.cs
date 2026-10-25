using AwesomeAssertions;
using Nakama.Formations.Application.DetailsUpdater;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationDetailsUpdaterShould : FormationsUnitTestCase
{
    private FormationDetailsUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailsUpdater.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task change_the_title_the_audience_and_the_description_and_publish_formation_details_updated()
    {
        Formation formation = FormationMother.Random();
        string title = $"{formation.Title} updated";
        string audience = $"{formation.Audience} updated";
        string description = $"{formation.Description} updated";
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationDetailsUpdaterCommandMother.With(formation.Id.Value, title, audience, description),
            CancellationToken.None);

        formation.Title.Should().Be(title);
        formation.Audience.Should().Be(audience);
        formation.Description.Should().Be(description);
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationDetailsUpdated>(
            @event => @event.Id == formation.Id
                      && @event.Title == title
                      && @event.Audience == audience
                      && @event.Description == description);
    }

    [Test]
    public async Task record_no_new_event_when_the_details_are_the_same()
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationDetailsUpdaterCommandMother.With(formation.Id.Value, formation.Title, formation.Audience, formation.Description),
            CancellationToken.None);

        formation.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationDetailsUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_is_archived()
    {
        Formation formation = FormationMother.Archived();
        this.ShouldFindFormation(formation);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationDetailsUpdaterCommandMother.With(formation.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<FormationArchivedError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
