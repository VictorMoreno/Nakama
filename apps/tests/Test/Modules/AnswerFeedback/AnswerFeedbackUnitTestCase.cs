using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.AnswerFeedback.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.AnswerFeedback;
using Test.Modules.Shared;

namespace Test.Modules.AnswerFeedback;

public abstract class AnswerFeedbackUnitTestCase : UnitTestCase
{
    private readonly List<UnhelpfulAnswersBetweenSpecification> _searches = [];

    protected Mock<IAnswerFeedbackRepository> FeedbackRepository { get; } = new();

    protected IOptions<AnswerFeedbackOptions> FeedbackOptions { get; } = Options.Create(AnswerFeedbackOptionsMother.Random());

    protected IReadOnlyList<UnhelpfulAnswersBetweenSpecification> Searches => this._searches;

    [SetUp]
    public void AnswerFeedbackSetUp()
    {
        this.FeedbackRepository.Reset();
        this._searches.Clear();
        this.FeedbackRepository
            .Setup(repository => repository.Save(It.IsAny<AnswerFeedbackEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldNotFindFeedback() =>
        this.FeedbackRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as AnswerFeedbackEntry);

    protected void ShouldFindFeedback(AnswerFeedbackEntry entry) =>
        this.FeedbackRepository
            .Setup(repository => repository.FindById(entry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

    protected void ShouldFindUnhelpfulAnswers(IReadOnlyList<AnswerFeedbackEntry> entries) =>
        this.FeedbackRepository
            .Setup(repository => repository.Search(
                It.IsAny<ISpecification<AnswerFeedbackEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ISpecification<AnswerFeedbackEntry>, CancellationToken>((specification, _) =>
                this._searches.Add((UnhelpfulAnswersBetweenSpecification)specification))
            .ReturnsAsync(entries);

    protected void ShouldCountAnswers(int useful, int total)
    {
        this.FeedbackRepository
            .Setup(repository => repository.Count(It.IsAny<UsefulAnswersSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(useful);
        this.FeedbackRepository
            .Setup(repository => repository.Count(It.IsAny<RatedAnswersSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(total);
    }

    protected void ShouldHaveSavedFeedbackMatching(Expression<Func<AnswerFeedbackEntry, bool>> matches) =>
        this.FeedbackRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);
}
