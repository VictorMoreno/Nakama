using Nakama.ChatbotMembers.Application.Handlers;
using Nakama.ChatbotMembers.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotMembers.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ChatbotMembers.Application;

[TestFixture]
public sealed class ChatbotMemberRemovedOnChatbotTeamMemberRemovedShould : ChatbotMembersUnitTestCase
{
    private ChatbotMemberRemovedOnChatbotTeamMemberRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotMemberRemovedOnChatbotTeamMemberRemoved(this.MembersRepository.Object);
    }

    [Test]
    public async Task remove_the_member_of_the_chatbot_and_the_user_of_the_removed_team_member()
    {
        ChatbotId chatbotId = ChatbotMother.RandomId();
        Guid userId = Guid.NewGuid();
        ChatbotMember member = ChatbotMemberMother.With(chatbotId: chatbotId.Value, userId: userId);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(chatbotId, userId: userId);
        this.ShouldFindChatbotMember(chatbotId.Value, userId, member);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        this.ShouldHaveRemovedChatbotMember(member);
    }

    [Test]
    public async Task remove_nothing_when_the_member_is_not_projected()
    {
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.Random();
        this.ShouldFindChatbotMember(@event.Id.Value, @event.UserId, null);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveRemovedAnyChatbotMember();
    }
}
