using MediatR;
using Nakama.Analytics.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Analytics.Application.Handlers;

using Analytics = Nakama.Analytics.Domain.Analytics;

/// <summary>
/// Feeds the singleton <see cref="Analytics"/> row from <see cref="ChatbotCreated"/>. Analytics
/// has no dedicated creation event of its own (it's fed by whichever of several event types
/// arrives first), so this handler is defensive: creates the row via <c>From</c> the first time
/// any feeding event arrives, or applies onto it if some other event got there first.
/// </summary>
public sealed class AnalyticsCreatedOnChatbotCreated(IAnalyticsRepository analyticsRepository)
    : INotificationHandler<DomainEventNotification<ChatbotCreated>>
{
    public async Task Handle(DomainEventNotification<ChatbotCreated> n, CancellationToken ct)
    {
        var botKey = n.DomainEvent.Key.Value;
        var entry = await analyticsRepository.FindSingleton(ct);
        if (entry is null)
        {
            entry = Analytics.CreateFromChatbotCreated(botKey);
        }
        else
        {
            entry.ApplyChatbotCreated(botKey);
        }

        await analyticsRepository.Save(entry, ct);
    }
}
