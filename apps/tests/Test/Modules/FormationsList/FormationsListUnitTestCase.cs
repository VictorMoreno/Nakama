using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationsList;

public abstract class FormationsListUnitTestCase : UnitTestCase
{
    protected Mock<IFormationSummaryRepository> SummariesRepository { get; } = new();

    protected Mock<IFormationEnrollmentRepository> EnrollmentsRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    protected IOptions<PaginationOptions> Pagination { get; } = Options.Create(PaginationOptionsMother.With(defaultPageSize: 12, maxPageSize: 50));

    [SetUp]
    public void FormationsListSetUp()
    {
        this.SummariesRepository.Reset();
        this.EnrollmentsRepository.Reset();
    }

    protected void ShouldFindAllFormationSummaries(IReadOnlyCollection<FormationSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryListingSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries.ToList());

    protected void ShouldFindFormationSummariesByIds(IReadOnlyCollection<FormationSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryByIdsSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries.ToList());

    protected void ShouldFindEnrollmentsOfTheUser(Guid userId, IReadOnlyList<Enrollment> enrollments) =>
        this.EnrollmentsRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<Enrollment>>(specification =>
                    specification is FormationEnrollmentByUserSpecification
                    && ((FormationEnrollmentByUserSpecification)specification).UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

    protected void ShouldHaveSearchedAllFormationSummaries() =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryListingSpecification),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedAllFormationSummaries() =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryListingSpecification),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveSearchedFormationSummariesByIds(IReadOnlyCollection<Guid> ids) =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => HasIds(specification, ids)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedFormationSummariesByIds() =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryByIdsSpecification),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindFormationSummary(FormationSummary summary) =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

    protected void ShouldNotFindFormationSummary() =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as FormationSummary);

    protected void ShouldHaveSavedFormationSummaryMatching(Expression<Func<FormationSummary, bool>> matches) =>
        this.SummariesRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveUpdatedFormationSummary(FormationSummary summary) =>
        this.SummariesRepository.Verify(
            repository => repository.Update(summary, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyFormationSummary() =>
        this.SummariesRepository.Verify(
            repository => repository.Update(It.IsAny<FormationSummary>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindFormationSummariesPage(IReadOnlyList<FormationSummary> summaries, int totalCount)
    {
        this.SummariesRepository
            .Setup(repository => repository.SearchPage(
                It.Is<ISpecification<FormationSummary>>(specification => specification is FormationSummaryPageSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<FormationSummary>(summaries, totalCount));
    }

    protected void ShouldHaveSearchedFormationSummariesPage(int skip, int take, IReadOnlyCollection<Guid>? ids) =>
        this.SummariesRepository.Verify(
            repository => repository.SearchPage(
                It.Is<ISpecification<FormationSummary>>(specification =>
                    IsPage(specification, skip, take) && HasPageIds(specification, ids)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedFormationSummariesPage() =>
        this.SummariesRepository.Verify(
            repository => repository.SearchPage(
                It.IsAny<ISpecification<FormationSummary>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    private static bool IsPage(ISpecification<FormationSummary> specification, int skip, int take) =>
        specification is FormationSummaryPageSpecification
        && specification.IsPagingEnabled
        && specification.Skip == skip
        && specification.Take == take;

    private static bool HasPageIds(ISpecification<FormationSummary> specification, IReadOnlyCollection<Guid>? ids)
    {
        IReadOnlyCollection<Guid>? actual = ((FormationSummaryPageSpecification)specification).Ids;

        return ids is null
            ? actual is null
            : actual is not null && actual.OrderBy(id => id).SequenceEqual(ids.OrderBy(id => id));
    }

    private static bool HasIds(ISpecification<FormationSummary> specification, IReadOnlyCollection<Guid> ids) =>
        specification is FormationSummaryByIdsSpecification byIds
        && byIds.Ids.OrderBy(id => id).SequenceEqual(ids.OrderBy(id => id));
}
