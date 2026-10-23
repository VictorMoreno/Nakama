using AwesomeAssertions;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Infrastructure;
using Nakama.Shared.Infrastructure;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared.Infrastructure;

namespace Test.Modules.Chatbots.Infrastructure;

[TestFixture]
public sealed class NhConversationsRepositoryShould : InfrastructureTestCase
{
    private static readonly TimeSpan DatabasePrecision = TimeSpan.FromMilliseconds(1);

    private NhConversationsRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhConversationsRepository(this.UnitOfWork);
    }

    [Test]
    public async Task keep_the_conversation_and_its_exchanges_with_and_without_attachments_in_order()
    {
        ConversationExchange withAttachment = ConversationExchangeMother.With(attachments: [ExchangeAttachmentMother.Random()]);
        ConversationExchange withoutAttachments = ConversationExchangeMother.With(attachments: null);
        Conversation conversation = ConversationMother.With(exchanges: [withAttachment, withoutAttachments]);
        await this._repository.Save(conversation, CancellationToken.None);
        await this.Persist();

        Conversation? found = await this._repository.FindById(conversation.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.BotKey.Should().Be(conversation.BotKey);
        found.UserId.Should().Be(conversation.UserId);
        found.Title.Should().Be(conversation.Title);
        found.StartedAt.Should().BeCloseTo(conversation.StartedAt, DatabasePrecision);
        found.Exchanges.Should().BeEquivalentTo(new[] { withAttachment, withoutAttachments }, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task add_an_exchange_to_a_conversation_that_was_already_saved()
    {
        ConversationExchange first = ConversationExchangeMother.Random();
        ConversationExchange second = ConversationExchangeMother.With(attachments: [ExchangeAttachmentMother.Random()]);
        Conversation conversation = ConversationMother.With(exchanges: [first]);
        await this._repository.Save(conversation, CancellationToken.None);
        await this.Persist();
        Conversation? stored = await this._repository.FindById(conversation.Id, CancellationToken.None);
        stored!.AppendExchange(second, new SystemClock());

        await this._repository.Save(stored, CancellationToken.None);
        await this.Persist();

        Conversation? found = await this._repository.FindById(conversation.Id, CancellationToken.None);
        found.Should().NotBeNull();
        found.Exchanges.Should().BeEquivalentTo(new[] { first, second }, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task read_exchanges_saved_before_attachments_existed_as_having_none()
    {
        Guid id = Guid.NewGuid();
        string legacyExchanges =
            "[{\"question\":\"old question\",\"answer\":\"old answer\",\"citation\":\"c\"," +
            "\"moreExplanation\":\"m\",\"socraticVariant\":\"s\"}]";
        await this.Execute(
            "INSERT INTO conversations (id, bot_key, user_id, title, startedat, exchanges_json) " +
            $"VALUES ('{id}', '{Guid.NewGuid()}', '{Guid.NewGuid()}', 'legacy', now(), '{legacyExchanges}')");

        Conversation? found = await this._repository.FindById(ConversationId.From(id), CancellationToken.None);

        found.Should().NotBeNull();
        ConversationExchange exchange = found.Exchanges.Should().ContainSingle().Which;
        exchange.Question.Should().Be("old question");
        exchange.Answer.Should().Be("old answer");
        exchange.Attachments.Should().BeNull();
    }

    [Test]
    public async Task remove_a_conversation_and_leave_the_others()
    {
        Conversation removed = ConversationMother.Random();
        Conversation kept = ConversationMother.Random();
        await this._repository.Save(removed, CancellationToken.None);
        await this._repository.Save(kept, CancellationToken.None);
        await this.Persist();

        await this._repository.Remove(removed, CancellationToken.None);
        await this.Persist();

        (await this._repository.FindById(removed.Id, CancellationToken.None)).Should().BeNull();
        (await this._repository.FindById(kept.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Test]
    public async Task find_nothing_for_a_conversation_that_was_never_saved()
    {
        await this._repository.Save(ConversationMother.Random(), CancellationToken.None);
        await this.Persist();

        Conversation? found = await this._repository.FindById(ConversationMother.RandomId(), CancellationToken.None);

        found.Should().BeNull();
    }
}
