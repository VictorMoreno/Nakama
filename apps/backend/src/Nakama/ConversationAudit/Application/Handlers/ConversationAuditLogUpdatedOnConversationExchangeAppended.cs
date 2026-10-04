using Nakama.ConversationAudit.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationAudit.Application.Handlers;

public sealed class ConversationAuditLogUpdatedOnConversationExchangeAppended(
    IConversationAuditRepository auditRepository,
    ISystemClock systemClock) : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended conversationExchangeAppended, CancellationToken cancellationToken)
    {
        var log = await auditRepository.FindById(conversationExchangeAppended.Id.Value, cancellationToken);
        if (log is null)
        {
            return;
        }

        log.AppendMessage(
            conversationExchangeAppended.Exchange.Question,
            conversationExchangeAppended.Exchange.Answer,
            conversationExchangeAppended.Exchange.Citation,
            conversationExchangeAppended.Exchange.MoreExplanation,
            conversationExchangeAppended.Exchange.SocraticVariant,
            systemClock.UtcNow);

        await auditRepository.Update(log, cancellationToken);
    }
}
