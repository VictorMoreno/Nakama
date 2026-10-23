using AwesomeAssertions;
using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Finder;

namespace Test.Modules.AnswerFeedback.Application;

[TestFixture]
public sealed class AnswerFeedbackSummaryFinderShould : AnswerFeedbackUnitTestCase
{
    [Test]
    public async Task count_the_useful_and_the_total_rated_answers()
    {
        AnswerFeedbackSummaryFinder.Handler handler = new(new(this.FeedbackRepository.Object));
        this.ShouldCountAnswers(useful: 6, total: 8);

        AnswerFeedbackSummaryResponse response = await handler.Handle(new AnswerFeedbackSummaryFinder.Query(), CancellationToken.None);

        response.Should().Be(new AnswerFeedbackSummaryResponse(6, 8));
    }
}
