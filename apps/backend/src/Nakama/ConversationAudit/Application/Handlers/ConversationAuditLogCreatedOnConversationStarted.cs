using Nakama.ConversationAudit.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationAudit.Application.Handlers;

public sealed class ConversationAuditLogCreatedOnConversationStarted(
    IConversationAuditRepository auditRepository,
    ISystemClock systemClock) : DomainEventHandler<ConversationStarted>
{
    protected override async Task Handle(ConversationStarted conversationStarted, CancellationToken cancellationToken)
    {
        // Defensive idempotency (same spirit as ChatPage's own handlers) - ConversationStarted
        // only ever fires once per conversation, but a retried delivery shouldn't duplicate the log.
        var existing = await auditRepository.FindById(conversationStarted.Id.Value, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var log = ConversationAuditLog.CreateFromConversationStarted(
            conversationStarted.Id.Value,
            conversationStarted.UserId,
            conversationStarted.BotKey.Value,
            conversationStarted.Title,
            systemClock.UtcNow);

        await auditRepository.Save(log, cancellationToken);
    }
}
