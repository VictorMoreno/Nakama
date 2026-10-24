using AwesomeAssertions;
using Nakama.Formations.Application.DocumentRemover;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationDocumentRemoverShould : FormationsUnitTestCase
{
    private FormationDocumentRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDocumentRemover.Handler(
            new(this.FormationsRepository.Object, this.DocumentStorage.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_document_with_that_id_and_delete_its_file()
    {
        FormationDocument first = FormationDocumentMother.Random();
        FormationDocument second = FormationDocumentMother.Random();
        Formation formation = FormationMother.With(documents: [first, second]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(FormationDocumentRemoverCommandMother.With(formation.Id.Value, second.Id), CancellationToken.None);

        formation.Documents.Should().ContainSingle().Which.Should().Be(first);
        this.ShouldHaveDeletedDocumentFile(second.FileName);
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationDocumentRemoved>(@event => @event.Id == formation.Id && @event.DocumentId == second.Id);
    }

    [Test]
    public async Task fail_without_deleting_any_file_when_no_document_has_that_id()
    {
        Formation formation = FormationMother.With(documents: [FormationDocumentMother.Random()]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            FormationDocumentRemoverCommandMother.With(formation.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationDocumentNotFoundError>();
        formation.Documents.Should().HaveCount(1);
        this.ShouldNotHaveDeletedAnyDocumentFile();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_deleting_any_file_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationDocumentRemoverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveDeletedAnyDocumentFile();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }
}
