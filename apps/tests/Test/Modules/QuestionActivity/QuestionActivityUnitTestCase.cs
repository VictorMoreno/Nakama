using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.QuestionActivity.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.QuestionActivity;
using Test.Modules.Shared;

namespace Test.Modules.QuestionActivity;

public abstract class QuestionActivityUnitTestCase : UnitTestCase
{
    private readonly List<QuestionActivityBetweenSpecification> _searches = [];

    protected Mock<IQuestionActivityRepository> QuestionRepository { get; } = new();

    protected IOptions<QuestionActivityOptions> QuestionOptions { get; } = Options.Create(QuestionActivityOptionsMother.Random());

    protected IReadOnlyList<QuestionActivityBetweenSpecification> Searches => this._searches;

    [SetUp]
    public void QuestionActivitySetUp()
    {
        this.QuestionRepository.Reset();
        this._searches.Clear();
        this.QuestionRepository
            .Setup(repository => repository.Save(It.IsAny<QuestionActivityEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldFindQuestions(IReadOnlyList<QuestionActivityEntry> entries) =>
        this.QuestionRepository
            .Setup(repository => repository.Search(
                It.IsAny<ISpecification<QuestionActivityEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ISpecification<QuestionActivityEntry>, CancellationToken>((specification, _) =>
                this._searches.Add((QuestionActivityBetweenSpecification)specification))
            .ReturnsAsync(entries);

    protected void ShouldHaveSavedQuestionActivityEntryMatching(Expression<Func<QuestionActivityEntry, bool>> matches) =>
        this.QuestionRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);
}
