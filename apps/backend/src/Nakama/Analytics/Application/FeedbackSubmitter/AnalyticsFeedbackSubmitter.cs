using Nakama.Analytics.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Analytics.Application.FeedbackSubmitter;

using Analytics = Nakama.Analytics.Domain.Analytics;

public static class AnalyticsFeedbackSubmitter
{
    public sealed record Command(bool Useful) : ICommand;

    public sealed class Handler(Submitter submitter) : ICommandHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken ct) => await submitter.Execute(command.Useful, ct);
    }

    public sealed class Submitter(IAnalyticsRepository repository) : IService
    {
        public async Task Execute(bool useful, CancellationToken ct)
        {
            Analytics? analytics = await repository.Find(ct);
            if (analytics is null)
            {
                analytics = Analytics.CreateFromFeedbackSubmitted(useful);
                await repository.Save(analytics, ct);
            }
            else
            {
                analytics.ApplyFeedbackSubmitted(useful);
                await repository.Update(analytics, ct);
            }
        }
    }
}
