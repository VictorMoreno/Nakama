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
public sealed class ChatbotDetailUpdatedOnChatbotDocumentAddedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotDocumentAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotDocumentAdded(this.DetailRepository.Object);
    }

    [Test]
    public async Task add_the_document_row_to_the_detail()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.Random();
        ChatbotDocument document = ChatbotDocumentMother.Random();
        ChatbotDocumentAdded @event = ChatbotDocumentAddedMother.With(ChatbotId.From(detail.Id), document);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentAdded>(@event), CancellationToken.None);

        ChatbotDocumentRow row = detail.Docs.Should().ContainSingle().Which;
        row.Id.Should().Be(document.Id);
        row.Name.Should().Be(document.Name);
        row.Origin.Should().Be(document.Origin);
        row.Updated.Should().Be(document.Updated);
        row.FileName.Should().Be(document.FileName);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task keep_the_documents_already_in_the_detail()
    {
        ChatbotDocumentRow existing = ChatbotDocumentRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(docs: [existing]);
        ChatbotDocumentAdded @event = ChatbotDocumentAddedMother.With(ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentAdded>(@event), CancellationToken.None);

        detail.Docs.Should().HaveCount(2);
        detail.Docs.First().Should().BeSameAs(existing);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotDocumentAdded @event = ChatbotDocumentAddedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
