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
public sealed class ChatbotDetailUpdatedOnChatbotIconUpdatedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotIconUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotIconUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task set_the_icon_of_the_detail()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(iconUrl: null);
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.With(ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        detail.IconUrl.Should().Be(@event.IconUrl);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task clear_the_icon_of_the_detail_when_the_event_has_none()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(iconUrl: "icon.png");
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.Cleared(ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        detail.IconUrl.Should().BeNull();
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
