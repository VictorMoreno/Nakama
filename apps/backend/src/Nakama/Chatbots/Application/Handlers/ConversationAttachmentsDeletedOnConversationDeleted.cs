using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Handlers;

public sealed class ConversationAttachmentsDeletedOnConversationDeleted(IConversationAttachmentStorage attachmentStorage)
    : DomainEventHandler<ConversationDeleted>
{
    protected override Task Handle(ConversationDeleted conversationDeleted, CancellationToken cancellationToken)
    {
        attachmentStorage.Delete(conversationDeleted.AttachmentFileNames);
        return Task.CompletedTask;
    }
}
