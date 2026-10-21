using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotMembers.Application.Searcher;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotSummaries;

public abstract class ChatbotSummariesUnitTestCase : UnitTestCase
{
    protected Mock<IChatbotSummaryRepository> SummariesRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    protected IOptions<PaginationOptions> Pagination { get; } = Options.Create(PaginationOptionsMother.With(defaultPageSize: 12, maxPageSize: 50));

    [SetUp]
    public void ChatbotSummariesSetUp() => this.SummariesRepository.Reset();

    protected void ShouldFindAllChatbotSummaries(IReadOnlyList<ChatbotSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<ChatbotSummary>>(specification => specification is ChatbotSummaryListingSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

    protected void ShouldFindChatbotSummary(ChatbotSummary summary) =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

    protected void ShouldNotFindChatbotSummary() =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ChatbotSummary);

    protected void ShouldHaveSavedChatbotSummaryMatching(Expression<Func<ChatbotSummary, bool>> matches) =>
        this.SummariesRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveUpdatedChatbotSummary(ChatbotSummary summary) =>
        this.SummariesRepository.Verify(
            repository => repository.Update(summary, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyChatbotSummary() =>
        this.SummariesRepository.Verify(
            repository => repository.Update(It.IsAny<ChatbotSummary>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindChatbotSummariesByIds(IReadOnlyList<ChatbotSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.Search(
                It.Is<ISpecification<ChatbotSummary>>(specification => specification is ChatbotSummaryByIdsSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

    protected void ShouldHaveSearchedChatbotSummariesByIds(IReadOnlyCollection<Guid> ids) =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<ChatbotSummary>>(specification => HasIds(specification, ids)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedAllChatbotSummaries() =>
        this.SummariesRepository.Verify(
            repository => repository.Search(
                It.Is<ISpecification<ChatbotSummary>>(specification => specification is ChatbotSummaryListingSpecification),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindMembershipsOfTheUser(Guid userId, IReadOnlyCollection<ChatbotMemberResponse> memberships) =>
        this.QueryBus
            .Setup(bus => bus.Ask(
                It.Is<ChatbotMemberSearcher.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberships);

    protected void ShouldNotHaveAskedForAnyMembership() =>
        this.QueryBus.Verify(
            bus => bus.Ask(It.IsAny<ChatbotMemberSearcher.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindChatbotSummariesPage(IReadOnlyList<ChatbotSummary> summaries, int totalCount)
    {
        this.SummariesRepository
            .Setup(repository => repository.SearchPage(
                It.Is<ISpecification<ChatbotSummary>>(specification => specification is ChatbotSummaryPageSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ChatbotSummary>(summaries, totalCount));
    }

    protected void ShouldHaveSearchedChatbotSummariesPage(int skip, int take, IReadOnlyCollection<Guid>? ids) =>
        this.SummariesRepository.Verify(
            repository => repository.SearchPage(
                It.Is<ISpecification<ChatbotSummary>>(specification =>
                    IsPage(specification, skip, take) && HasPageIds(specification, ids)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSearchedChatbotSummariesPage() =>
        this.SummariesRepository.Verify(
            repository => repository.SearchPage(
                It.IsAny<ISpecification<ChatbotSummary>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    private static bool IsPage(ISpecification<ChatbotSummary> specification, int skip, int take) =>
        specification is ChatbotSummaryPageSpecification
        && specification.IsPagingEnabled
        && specification.Skip == skip
        && specification.Take == take;

    private static bool HasPageIds(ISpecification<ChatbotSummary> specification, IReadOnlyCollection<Guid>? ids)
    {
        IReadOnlyCollection<Guid>? actual = ((ChatbotSummaryPageSpecification)specification).Ids;

        return ids is null
            ? actual is null
            : actual is not null && actual.OrderBy(id => id).SequenceEqual(ids.OrderBy(id => id));
    }

    private static bool HasIds(ISpecification<ChatbotSummary> specification, IReadOnlyCollection<Guid> ids) =>
        specification is ChatbotSummaryByIdsSpecification byIds
        && byIds.Ids.OrderBy(id => id).SequenceEqual(ids.OrderBy(id => id));
}
