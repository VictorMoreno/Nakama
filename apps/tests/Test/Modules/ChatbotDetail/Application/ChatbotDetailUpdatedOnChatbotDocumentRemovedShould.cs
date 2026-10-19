using AwesomeAssertions;
using Nakama.ChatbotDetail.Application.Handlers;
using Nakama.ChatbotDetail.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotDetail.Domain;
using Test.Modules.Chatbots.Domain;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Application;

[TestFixture]
public sealed class ChatbotDetailUpdatedOnChatbotDocumentRemovedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotDocumentRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotDocumentRemoved(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_document_row_with_that_id()
    {
        ChatbotDocumentRow first = ChatbotDocumentRowMother.Random();
        ChatbotDocumentRow second = ChatbotDocumentRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(docs: [first, second]);
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.With(ChatbotId.From(detail.Id), first.Id);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        detail.Docs.Should().ContainSingle().Which.Should().BeSameAs(second);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task leave_the_documents_untouched_when_no_row_has_that_id()
    {
        ChatbotDocumentRow document = ChatbotDocumentRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(docs: [document]);
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.With(ChatbotId.From(detail.Id), Guid.NewGuid());
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        detail.Docs.Should().ContainSingle().Which.Should().BeSameAs(document);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
