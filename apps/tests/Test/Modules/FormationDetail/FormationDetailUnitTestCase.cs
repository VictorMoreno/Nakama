using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail;

public abstract class FormationDetailUnitTestCase : UnitTestCase
{
    protected Mock<IFormationDetailRepository> DetailRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    [SetUp]
    public void FormationDetailSetUp() => this.DetailRepository.Reset();

    protected void ShouldFindFormationDetail(FormationDetailEntity detail) =>
        this.DetailRepository
            .Setup(repository => repository.FindById(detail.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

    protected void ShouldNotFindFormationDetail() =>
        this.DetailRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as FormationDetailEntity);

    protected void ShouldHaveSavedFormationDetailMatching(Expression<Func<FormationDetailEntity, bool>> matches) =>
        this.DetailRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveUpdatedFormationDetail(FormationDetailEntity detail) =>
        this.DetailRepository.Verify(
            repository => repository.Update(detail, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyFormationDetail() =>
        this.DetailRepository.Verify(
            repository => repository.Update(It.IsAny<FormationDetailEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
