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
public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberRemovedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotTeamMemberRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotTeamMemberRemoved(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_member_row_with_that_user_id()
    {
        TeamMemberRow first = TeamMemberRowMother.Random();
        TeamMemberRow second = TeamMemberRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(team: [first, second]);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(detail.Id), second.UserId);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(first);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task leave_the_team_untouched_when_no_row_has_that_user_id()
    {
        TeamMemberRow member = TeamMemberRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(team: [member]);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(detail.Id), Guid.NewGuid());
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(member);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
