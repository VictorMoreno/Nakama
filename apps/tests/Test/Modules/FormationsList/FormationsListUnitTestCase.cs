using Microsoft.Extensions.Options;
using Moq;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationsList;

public abstract class FormationsListUnitTestCase : UnitTestCase
{
    protected Mock<IFormationSummaryRepository> SummariesRepository { get; } = new();

    protected Mock<IFormationEnrollmentRepository> EnrollmentsRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    [SetUp]
    public void FormationsListSetUp()
    {
        this.SummariesRepository.Reset();
        this.EnrollmentsRepository.Reset();
    }

    protected void ShouldFindAllFormationSummaries(IReadOnlyCollection<FormationSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.FindAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

    protected void ShouldFindFormationSummariesByIds(IReadOnlyCollection<FormationSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.FindByIds(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

    protected void ShouldFindEnrollmentsOfTheUser(Guid userId, IReadOnlyList<Enrollment> enrollments) =>
        this.EnrollmentsRepository
            .Setup(repository => repository.FindAllForUser(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

    protected void ShouldHaveSearchedAllFormationSummaries() =>
        this.SummariesRepository.Verify(
            repository => repository.FindAll(It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedAllFormationSummaries() =>
        this.SummariesRepository.Verify(
            repository => repository.FindAll(It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveSearchedFormationSummariesByIds(IReadOnlyCollection<Guid> ids) =>
        this.SummariesRepository.Verify(
            repository => repository.FindByIds(
                It.Is<IReadOnlyCollection<Guid>>(received => received.OrderBy(id => id).SequenceEqual(ids.OrderBy(id => id))),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedFormationSummariesByIds() =>
        this.SummariesRepository.Verify(
            repository => repository.FindByIds(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
