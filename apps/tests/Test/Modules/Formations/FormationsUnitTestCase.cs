using System.Linq.Expressions;
using Moq;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.Formations.Application;
using Nakama.Formations.Application.DocumentFileFinder;
using Nakama.Formations.Domain;
using Nakama.Shared.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations;

public abstract class FormationsUnitTestCase : UnitTestCase
{
    protected Mock<IFormationsRepository> FormationsRepository { get; } = new();

    protected Mock<IFormationIconStorage> IconStorage { get; } = new();

    protected Mock<IFormationDocumentStorage> DocumentStorage { get; } = new();

    protected Mock<IFormationDocumentFileReader> DocumentFileReader { get; } = new();

    [SetUp]
    public void FormationsSetUp()
    {
        this.FormationsRepository.Reset();
        this.IconStorage.Reset();
        this.DocumentStorage.Reset();
        this.DocumentFileReader.Reset();
    }

    protected void ShouldFindFormation(Formation formation) =>
        this.FormationsRepository
            .Setup(repository => repository.FindById(formation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(formation);

    protected void ShouldNotFindFormation() =>
        this.FormationsRepository
            .Setup(repository => repository.FindById(It.IsAny<FormationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Formation);

    protected void ShouldFindFormationDetail(FormationDetailResponse formation) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<FormationDetailFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(formation);

    protected void ShouldFindDocumentFile(FormationDocumentResponse document, string contentType) =>
        this.QueryBus
            .Setup(bus => bus.Ask(
                It.Is<FormationDocumentFileFinder.Query>(query => query.DocumentId == document.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormationDocumentFileResponse([1, 2, 3], contentType, document.FileName));

    protected void ShouldStoreIcon(string fileName) =>
        this.IconStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileName);

    protected void ShouldStoreDocument(string fileName) =>
        this.DocumentStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileName);

    protected void ShouldFailToStoreDocument(DomainError error) =>
        this.DocumentStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(error);

    protected void ShouldReadDocumentFile(byte[]? content) =>
        this.DocumentFileReader
            .Setup(reader => reader.Read(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

    protected void ShouldHaveSavedFormation(Formation formation) =>
        this.FormationsRepository.Verify(
            repository => repository.Save(formation, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedFormationMatching(Expression<Func<Formation, bool>> matches) =>
        this.FormationsRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyFormation() =>
        this.FormationsRepository.Verify(
            repository => repository.Save(It.IsAny<Formation>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveStoredIcon(Guid id, string contentType) =>
        this.IconStorage.Verify(
            storage => storage.Save(
                id,
                contentType,
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveStoredDocument(Guid documentId, string originalFileName, long length) =>
        this.DocumentStorage.Verify(
            storage => storage.Save(
                documentId,
                originalFileName,
                It.IsAny<Stream>(),
                length,
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveStoredAnyDocument() =>
        this.DocumentStorage.Verify(
            storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveDeletedDocumentFile(string fileName) =>
        this.DocumentStorage.Verify(storage => storage.Delete(fileName), Times.Once);

    protected void ShouldNotHaveDeletedAnyDocumentFile() =>
        this.DocumentStorage.Verify(storage => storage.Delete(It.IsAny<string>()), Times.Never);

    protected void ShouldHaveDeletedExistingIcon(Guid id) => this.IconStorage.Verify(storage => storage.DeleteExisting(id), Times.Once);

    protected void ShouldNotHaveTouchedTheIconStorage()
    {
        this.IconStorage.Verify(
            storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        this.IconStorage.Verify(storage => storage.DeleteExisting(It.IsAny<Guid>()), Times.Never);
    }
}
