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
public sealed class ChatbotDetailUpdatedOnChatbotStatusChangedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotStatusChanged _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotStatusChanged(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_the_status_of_the_detail()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(status: ChatbotStatus.Active.ToString());
        ChatbotStatusChanged @event = ChatbotStatusChangedMother.With(ChatbotId.From(detail.Id), ChatbotStatus.Draft);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotStatusChanged>(@event), CancellationToken.None);

        detail.Status.Should().Be(ChatbotStatus.Draft.ToString());
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotStatusChanged @event = ChatbotStatusChangedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotStatusChanged>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
