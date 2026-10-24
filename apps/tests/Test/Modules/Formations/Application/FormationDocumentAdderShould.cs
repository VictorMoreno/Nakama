using AwesomeAssertions;
using Nakama.Formations.Application.DocumentAdder;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationDocumentAdderShould : FormationsUnitTestCase
{
    private FormationDocumentAdder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDocumentAdder.Handler(new(
            this.FormationsRepository.Object,
            this.DocumentStorage.Object,
            this.EventBus.Object,
            this.Clock.Object));
    }

    [Test]
    public async Task store_the_file_and_add_the_document_to_the_formation()
    {
        DateTime now = DateTime.UtcNow;
        string fileName = $"{Guid.NewGuid():N}.pdf";
        Formation formation = FormationMother.Random();
        FormationDocumentAdder.Command command = FormationDocumentAdderCommandMother.With(formation.Id.Value);
        this.ShouldBeNow(now);
        this.ShouldFindFormation(formation);
        this.ShouldStoreDocument(fileName);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Documents.Should().ContainSingle().Which.Should().Be(new FormationDocument(
            command.DocumentId,
            command.Name,
            command.Origin,
            now,
            fileName));
        this.ShouldHaveStoredDocument(command.DocumentId, command.OriginalFileName, command.Length);
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationDocumentAdded>(
            @event => @event.Id == formation.Id && @event.Document.Id == command.DocumentId);
    }

    [Test]
    public async Task fail_without_storing_the_file_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationDocumentAdderCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveStoredAnyDocument();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_adding_the_document_when_the_file_cannot_be_stored()
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);
        this.ShouldFailToStoreDocument(FormationErrors.DocumentUnsupportedType);

        Func<Task> act = async () => await this._handler.Handle(
            FormationDocumentAdderCommandMother.With(formation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationDocumentUnsupportedTypeError>();
        formation.Documents.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
