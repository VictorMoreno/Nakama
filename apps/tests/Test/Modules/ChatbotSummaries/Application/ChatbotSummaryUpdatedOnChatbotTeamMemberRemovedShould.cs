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
public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberRemovedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotTeamMemberRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotTeamMemberRemoved(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_team_member_less()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(teamCount: 3);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        summary.TeamCount.Should().Be(2);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task keep_the_team_count_at_zero_when_there_are_no_members()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(teamCount: 0);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        summary.TeamCount.Should().Be(0);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
