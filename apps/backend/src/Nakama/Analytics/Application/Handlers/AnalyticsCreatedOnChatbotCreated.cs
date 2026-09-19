using Nakama.Analytics.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Analytics.Application.Handlers;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class AnalyticsCreatedOnChatbotCreated(IAnalyticsRepository analyticsRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override async Task Handle(ChatbotCreated chatbotCreated, CancellationToken ct)
    {
        var botKey = chatbotCreated.Id.Value;
        var analytics = await analyticsRepository.Find(ct);
        if (analytics is null)
        {
            analytics = Analytics.CreateFromChatbotCreated(botKey);
            await analyticsRepository.Save(analytics, ct);
        }
        else
        {
            analytics.ApplyChatbotCreated(botKey);
            await analyticsRepository.Update(analytics, ct);
        }
    }
}
