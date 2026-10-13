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
}
