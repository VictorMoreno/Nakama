using System.Linq.Expressions;
using Moq;
using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations;

public abstract class FormationsUnitTestCase : UnitTestCase
{
    protected Mock<IFormationsRepository> FormationsRepository { get; } = new();

    protected Mock<IFormationIconStorage> IconStorage { get; } = new();

    [SetUp]
    public void FormationsSetUp()
    {
        this.FormationsRepository.Reset();
        this.IconStorage.Reset();
    }

    protected void ShouldFindFormation(Formation? formation) =>
        this.FormationsRepository
            .Setup(repository => repository.FindById(It.IsAny<FormationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(formation);

    protected void ShouldStoreIcon(string fileName) =>
        this.IconStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileName);

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
