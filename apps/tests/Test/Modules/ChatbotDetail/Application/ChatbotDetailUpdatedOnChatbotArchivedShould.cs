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
public sealed class ChatbotDetailUpdatedOnChatbotArchivedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotArchived _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotArchived(this.DetailRepository.Object);
    }

    [Test]
    public async Task mark_the_detail_as_archived()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(archived: false);
        ChatbotArchived @event = ChatbotArchivedMother.With(ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotArchived>(@event), CancellationToken.None);

        detail.Archived.Should().BeTrue();
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotArchived @event = ChatbotArchivedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotArchived>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
