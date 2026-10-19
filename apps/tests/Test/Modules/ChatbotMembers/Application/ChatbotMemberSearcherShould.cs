using AwesomeAssertions;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotMembers.Application.Searcher;
using Nakama.ChatbotMembers.Domain;
using Test.Modules.ChatbotMembers.Domain;

namespace Test.Modules.ChatbotMembers.Application;

[TestFixture]
public sealed class ChatbotMemberSearcherShould : ChatbotMembersUnitTestCase
{
    private ChatbotMemberSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotMemberSearcher.Handler(new(this.MembersRepository.Object));
    }

    [Test]
    public async Task return_the_chatbots_the_user_is_a_member_of()
    {
        Guid userId = Guid.NewGuid();
        ChatbotMember first = ChatbotMemberMother.With(userId: userId);
        ChatbotMember second = ChatbotMemberMother.With(userId: userId);
        this.ShouldFindMembershipsOfTheUser(userId, [first, second]);

        IReadOnlyCollection<ChatbotMemberResponse> response = await this._handler.Handle(
            new ChatbotMemberSearcher.Query(userId),
            CancellationToken.None);

        response.Select(item => (item.ChatbotId, item.UserId)).Should().Equal(
            (first.ChatbotId, userId),
            (second.ChatbotId, userId));
    }

    [Test]
    public async Task return_nothing_when_the_user_is_not_a_member_of_any_chatbot()
    {
        Guid userId = Guid.NewGuid();
        this.ShouldFindMembershipsOfTheUser(userId, []);

        IReadOnlyCollection<ChatbotMemberResponse> response = await this._handler.Handle(
            new ChatbotMemberSearcher.Query(userId),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
