using AwesomeAssertions;
using Nakama.Formations.Application.IconUpdater;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationIconUpdaterShould : FormationsUnitTestCase
{
    private FormationIconUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationIconUpdater.Handler(
            new(this.FormationsRepository.Object, this.IconStorage.Object, this.EventBus.Object));
    }

    [Test]
    public async Task store_the_uploaded_icon_and_publish_formation_icon_updated()
    {
        string iconFileName = $"{WordMother.Random()}.png";
        Formation formation = FormationMother.Random();
        FormationIconUpdater.Command command = FormationIconUpdaterCommandMother.WithUpload(formation.Id.Value);
        this.ShouldFindFormation(formation);
        this.ShouldStoreIcon(iconFileName);

        await this._handler.Handle(command, CancellationToken.None);

        formation.IconUrl.Should().Be(iconFileName);
        this.ShouldHaveStoredIcon(formation.Id.Value, "image/png");
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationIconUpdated>(
            @event => @event.Id == formation.Id && @event.IconUrl == iconFileName);
    }

    [Test]
    public async Task delete_the_existing_icon_when_no_file_is_uploaded()
    {
        Formation formation = FormationMother.With(iconUrl: $"{WordMother.Random()}.png");
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationIconUpdaterCommandMother.WithoutUpload(formation.Id.Value),
            CancellationToken.None);

        formation.IconUrl.Should().BeNull();
        this.ShouldHaveDeletedExistingIcon(formation.Id.Value);
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationIconUpdated>(@event => @event.Id == formation.Id && @event.IconUrl is null);
    }

    [Test]
    public async Task fail_without_touching_the_storage_when_the_formation_does_not_exist()
    {
        this.ShouldFindFormation(null);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationIconUpdaterCommandMother.WithUpload(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveTouchedTheIconStorage();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
