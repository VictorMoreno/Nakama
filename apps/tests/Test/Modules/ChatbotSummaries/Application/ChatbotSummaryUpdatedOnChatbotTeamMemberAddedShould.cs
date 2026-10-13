using AwesomeAssertions;
using Nakama.ChatbotSummaries.Application.Handlers;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotSummaries.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ChatbotSummaries.Application;

[TestFixture]
public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberAddedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotTeamMemberAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotTeamMemberAdded(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_more_team_member()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(teamCount: 2);
        ChatbotTeamMemberAdded @event = ChatbotTeamMemberAddedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberAdded>(@event), CancellationToken.None);

        summary.TeamCount.Should().Be(3);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotTeamMemberAdded @event = ChatbotTeamMemberAddedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
