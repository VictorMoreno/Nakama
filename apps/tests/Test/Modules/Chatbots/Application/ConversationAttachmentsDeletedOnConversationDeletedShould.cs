using Nakama.Chatbots.Application.Handlers;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ConversationAttachmentsDeletedOnConversationDeletedShould : ChatbotsUnitTestCase
{
    private ConversationAttachmentsDeletedOnConversationDeleted _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationAttachmentsDeletedOnConversationDeleted(this.AttachmentStorage.Object);
    }

    [Test]
    public async Task delete_the_attachment_files_of_the_deleted_conversation()
    {
        string first = $"{WordMother.Random()}.pdf";
        string second = $"{WordMother.Random()}.txt";
        ConversationDeleted @event = ConversationDeletedMother.With(attachmentFileNames: [first, second]);

        await this._handler.Handle(new DomainEventNotification<ConversationDeleted>(@event), CancellationToken.None);

        this.ShouldHaveDeletedAttachments([first, second]);
    }

    [Test]
    public async Task ask_the_storage_to_delete_nothing_when_the_conversation_had_no_attachments()
    {
        ConversationDeleted @event = ConversationDeletedMother.With(attachmentFileNames: []);

        await this._handler.Handle(new DomainEventNotification<ConversationDeleted>(@event), CancellationToken.None);

        this.ShouldHaveDeletedAttachments([]);
    }
}
