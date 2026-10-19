using Nakama.ChatbotMembers.Application.Handlers;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ChatbotMembers.Application;

[TestFixture]
public sealed class ChatbotMemberCreatedOnChatbotTeamMemberAddedShould : ChatbotMembersUnitTestCase
{
    private ChatbotMemberCreatedOnChatbotTeamMemberAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotMemberCreatedOnChatbotTeamMemberAdded(this.MembersRepository.Object);
    }

    [Test]
    public async Task save_a_member_with_the_chatbot_and_the_user_of_the_added_team_member()
    {
        ChatbotTeamMemberAdded @event = ChatbotTeamMemberAddedMother.Random();

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberAdded>(@event), CancellationToken.None);

        this.ShouldHaveSavedChatbotMemberMatching(member =>
            member.ChatbotId == @event.Id.Value && member.UserId == @event.Member.UserId);
    }
}
