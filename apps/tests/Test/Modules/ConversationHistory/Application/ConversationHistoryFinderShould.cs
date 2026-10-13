using AwesomeAssertions;
using Nakama.ConversationHistory.Application;
using Nakama.ConversationHistory.Application.Finder;
using Nakama.ConversationHistory.Domain;
using Nakama.ConversationHistory.Domain.Errors;
using Test.Modules.ConversationHistory.Domain;
using ConversationHistoryEntity = Nakama.ConversationHistory.Domain.ConversationHistory;

namespace Test.Modules.ConversationHistory.Application;

[TestFixture]
public sealed class ConversationHistoryFinderShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryFinder.Handler(new(this.HistoriesRepository.Object));
    }

    [Test]
    public async Task return_only_the_conversations_of_the_requesting_user()
    {
        Guid userId = Guid.NewGuid();
        ConversationSummaryRow first = ConversationSummaryRowMother.With(userId: userId);
        ConversationSummaryRow second = ConversationSummaryRowMother.With(userId: userId);
        ConversationSummaryRow ofAnotherUser = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [first, ofAnotherUser, second]);
        this.ShouldFindConversationHistory(history);

        ConversationHistoryResponse response = await this._handler.Handle(
            ConversationHistoryFinderQueryMother.With(history.Id, userId),
            CancellationToken.None);

        response.BotKey.Should().Be(history.Id);
        response.Entries.Should().Equal(first, second);
    }

    [Test]
    public async Task keep_the_exchanges_of_the_conversations_of_the_user()
    {
        Guid userId = Guid.NewGuid();
        HistoryExampleRow exchange = HistoryExampleRowMother.With(attachments: ["notes.pdf"]);
        ConversationSummaryRow entry = ConversationSummaryRowMother.With(userId: userId, exchanges: [exchange]);
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        this.ShouldFindConversationHistory(history);

        ConversationHistoryResponse response = await this._handler.Handle(
            ConversationHistoryFinderQueryMother.With(history.Id, userId),
            CancellationToken.None);

        response.Entries.Should().ContainSingle().Which.Exchanges.Should().Equal(exchange);
    }

    [Test]
    public async Task return_no_conversations_when_the_user_has_none_in_this_history()
    {
        ConversationHistoryEntity history = ConversationHistoryMother.With(
            entries: [ConversationSummaryRowMother.Random(), ConversationSummaryRowMother.Random()]);
        this.ShouldFindConversationHistory(history);

        ConversationHistoryResponse response = await this._handler.Handle(
            ConversationHistoryFinderQueryMother.With(history.Id),
            CancellationToken.None);

        response.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task return_no_conversations_when_the_query_has_no_user()
    {
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [ConversationSummaryRowMother.Random()]);
        this.ShouldFindConversationHistory(history);

        ConversationHistoryResponse response = await this._handler.Handle(
            ConversationHistoryFinderQueryMother.WithoutUser(history.Id),
            CancellationToken.None);

        response.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_there_is_no_history_for_the_chatbot()
    {
        this.ShouldNotFindConversationHistory();

        Func<Task> act = async () =>
            await this._handler.Handle(ConversationHistoryFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ConversationHistoryNotFoundError>();
    }
}
