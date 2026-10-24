using AwesomeAssertions;
using Nakama.Formations.Application;
using Nakama.Formations.Application.DocumentFileFinder;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationDocumentFileFinderShould : FormationsUnitTestCase
{
    private FormationDocumentFileFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDocumentFileFinder.Handler(
            new(this.FormationsRepository.Object, this.DocumentFileReader.Object));
    }

    [TestCase("notes.pdf", "application/pdf")]
    [TestCase("notes.md", "text/markdown")]
    [TestCase("notes.txt", "text/plain")]
    public async Task return_the_content_with_the_content_type_of_its_extension(string fileName, string contentType)
    {
        FormationDocument document = FormationDocumentMother.With(name: "Guide", fileName: fileName);
        Formation formation = FormationMother.With(documents: [document]);
        byte[] content = [1, 2, 3];
        this.ShouldFindFormation(formation);
        this.ShouldReadDocumentFile(content);

        FormationDocumentFileResponse response = await this._handler.Handle(
            new FormationDocumentFileFinder.Query(formation.Id.Value, document.Id), CancellationToken.None);

        response.Content.Should().Equal(content);
        response.ContentType.Should().Be(contentType);
        response.FileName.Should().Be($"Guide{Path.GetExtension(fileName)}");
    }

    [Test]
    public async Task fail_when_the_formation_has_no_document_with_that_id()
    {
        Formation formation = FormationMother.With(documents: [FormationDocumentMother.Random()]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            new FormationDocumentFileFinder.Query(formation.Id.Value, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<FormationDocumentNotFoundError>();
    }

    [Test]
    public async Task fail_when_the_file_is_no_longer_in_the_storage()
    {
        FormationDocument document = FormationDocumentMother.Random();
        Formation formation = FormationMother.With(documents: [document]);
        this.ShouldFindFormation(formation);
        this.ShouldReadDocumentFile(null);

        Func<Task> act = async () => await this._handler.Handle(
            new FormationDocumentFileFinder.Query(formation.Id.Value, document.Id), CancellationToken.None);

        await act.Should().ThrowAsync<FormationDocumentNotFoundError>();
    }

    [Test]
    public async Task fail_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () => await this._handler.Handle(
            new FormationDocumentFileFinder.Query(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
    }
}
