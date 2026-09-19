using Nakama.Analytics.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Analytics.Application.Handlers;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class AnalyticsCreatedOnConversationExchangeAppended(IAnalyticsRepository analyticsRepository)
    : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended conversationExchangeAppended,
        CancellationToken ct)
    {
        var botKey = conversationExchangeAppended.BotKey.Value;
        Analytics? analytics = await analyticsRepository.Find(ct);
        if (analytics is null)
        {
            analytics = Analytics.CreateFromConversationExchangeAppended(botKey);
            await analyticsRepository.Save(analytics, ct);
        }
        else
        {
            analytics.ApplyConversationExchangeAppended(botKey);
            await analyticsRepository.Update(analytics, ct);
        }
    }
}