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
public sealed class ChatbotDetailUpdatedOnConversationExchangeAppendedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnConversationExchangeAppended _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnConversationExchangeAppended(this.DetailRepository.Object);
    }

    [Test]
    public async Task count_a_new_question_once_in_the_top_questions_of_the_chatbot()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.Random();
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(botKey: ChatbotId.From(detail.Id));
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        GapQuestionRow row = detail.TopQuestions.Should().ContainSingle().Which;
        row.Question.Should().Be(@event.Exchange.Question);
        row.Count.Should().Be(1);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    public async Task increase_the_count_of_a_question_already_asked()
    {
        ConversationExchange exchange = ConversationExchangeMother.Random();
        GapQuestionRow asked = GapQuestionRowMother.With(question: exchange.Question, count: 2);
        GapQuestionRow other = GapQuestionRowMother.With(count: 5);
        ChatbotDetailEntity detail = ChatbotDetailMother.With(topQuestions: [asked, other]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            botKey: ChatbotId.From(detail.Id),
            exchange: exchange);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        detail.TopQuestions.Should().HaveCount(2);
        asked.Count.Should().Be(3);
        other.Count.Should().Be(5);
    }

    [Test]
    public async Task add_a_different_question_next_to_the_existing_ones()
    {
        ConversationExchange exchange = ConversationExchangeMother.Random();
        GapQuestionRow existing = GapQuestionRowMother.With(question: $"{exchange.Question}-other", count: 4);
        ChatbotDetailEntity detail = ChatbotDetailMother.With(topQuestions: [existing]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            botKey: ChatbotId.From(detail.Id),
            exchange: exchange);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        detail.TopQuestions.Should().HaveCount(2);
        existing.Count.Should().Be(4);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
