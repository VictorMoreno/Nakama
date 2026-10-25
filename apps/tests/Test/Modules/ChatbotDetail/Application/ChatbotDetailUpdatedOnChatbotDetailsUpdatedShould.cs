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
public sealed class ChatbotDetailUpdatedOnChatbotDetailsUpdatedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotDetailsUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotDetailsUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_the_label_and_the_description_of_the_detail()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.Random();
        string label = $"{detail.Label} updated";
        string description = $"{detail.Description} updated";
        ChatbotDetailsUpdated @event = ChatbotDetailsUpdatedMother.With(ChatbotId.From(detail.Id), label, description);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotDetailsUpdated>(@event), CancellationToken.None);

        detail.Label.Should().Be(label);
        detail.Description.Should().Be(description);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotDetailsUpdated @event = ChatbotDetailsUpdatedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotDetailsUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
