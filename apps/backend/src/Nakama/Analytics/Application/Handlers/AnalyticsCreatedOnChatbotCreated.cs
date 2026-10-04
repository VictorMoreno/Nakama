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
        var botKey = chatbotCreated.Id.Value;
        var analytics = await analyticsRepository.Find(cancellationToken);
        if (analytics is null)
        {
            analytics = Analytics.CreateFromChatbotCreated(botKey);
            await analyticsRepository.Save(analytics, cancellationToken);
        }
        else
        {
            analytics.ApplyChatbotCreated(botKey);
            await analyticsRepository.Update(analytics, cancellationToken);
        }
    }
}
