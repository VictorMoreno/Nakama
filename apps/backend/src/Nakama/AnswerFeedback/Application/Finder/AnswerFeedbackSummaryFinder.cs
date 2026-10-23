using Nakama.AnswerFeedback.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.AnswerFeedback.Application.Finder;

public static class AnswerFeedbackSummaryFinder
{
    public sealed record Query : IQuery<AnswerFeedbackSummaryResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, AnswerFeedbackSummaryResponse>
    {
        public async Task<AnswerFeedbackSummaryResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(ct);
    }

    public sealed class Finder(IAnswerFeedbackRepository repository) : IService
    {
        public async Task<AnswerFeedbackSummaryResponse> Execute(CancellationToken cancellationToken)
        {
            int useful = await repository.Count(new UsefulAnswersSpecification(), cancellationToken);
            int total = await repository.Count(new RatedAnswersSpecification(), cancellationToken);

            return new AnswerFeedbackSummaryResponse(useful, total);
        }
    }
}
