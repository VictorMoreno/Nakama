using AwesomeAssertions;
using Nakama.ChatbotDetail.Application.Handlers;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotDetail.Domain;
using Test.Modules.Chatbots.Domain;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Application;

[TestFixture]
public sealed class ChatbotDetailUpdatedOnChatbotUnarchivedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotUnarchived _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotUnarchived(this.DetailRepository.Object);
    }

    [Test]
    public async Task mark_the_detail_as_unarchived()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(archived: true);
        ChatbotUnarchived @event = ChatbotUnarchivedMother.With(ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotUnarchived>(@event), CancellationToken.None);

        detail.Archived.Should().BeFalse();
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotUnarchived @event = ChatbotUnarchivedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotUnarchived>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
