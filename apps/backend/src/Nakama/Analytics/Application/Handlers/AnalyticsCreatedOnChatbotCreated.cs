using Nakama.Analytics.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Analytics.Application.Handlers;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class AnalyticsCreatedOnChatbotCreated(IAnalyticsRepository analyticsRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override async Task Handle(ChatbotCreated chatbotCreated, CancellationToken cancellationToken)
    {
        Analytics? analytics = await analyticsRepository.Find(cancellationToken);
        if (analytics is null)
        {
            analytics = Analytics.CreateFromChatbotCreated();
            await analyticsRepository.Save(analytics, cancellationToken);
        }
        else
        {
            analytics.ApplyChatbotCreated();
            await analyticsRepository.Update(analytics, cancellationToken);
        }
    }
}
