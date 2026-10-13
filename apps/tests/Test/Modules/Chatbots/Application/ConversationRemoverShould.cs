using AwesomeAssertions;
using Nakama.Chatbots.Application.Remover;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ConversationRemoverShould : ChatbotsUnitTestCase
{
    private ConversationRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationRemover.Handler(new(this.ConversationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_conversation_and_publish_conversation_deleted_when_it_belongs_to_the_user()
    {
        Conversation conversation = ConversationMother.Random();
        this.ShouldFindConversation(conversation);

        await this._handler.Handle(
            ConversationRemoverCommandMother.With(conversation.Id.Value, conversation.UserId),
            CancellationToken.None);

        this.ShouldHaveRemoved(conversation);
        this.ShouldHavePublishedConversationDeleted(conversation.Id, []);
    }

    [Test]
    public async Task publish_the_attachment_files_of_every_exchange_so_they_can_be_deleted()
    {
        ExchangeAttachment first = ExchangeAttachmentMother.Random();
        ExchangeAttachment second = ExchangeAttachmentMother.Random();
        Conversation conversation = ConversationMother.With(exchanges:
        [
            ConversationExchangeMother.With(attachments: [first]),
            ConversationExchangeMother.Random(),
            ConversationExchangeMother.With(attachments: [second])
        ]);
        this.ShouldFindConversation(conversation);

        await this._handler.Handle(
            ConversationRemoverCommandMother.With(conversation.Id.Value, conversation.UserId),
            CancellationToken.None);

        this.ShouldHavePublishedConversationDeleted(conversation.Id, [first.FileName, second.FileName]);
    }

    [Test]
    public async Task fail_without_removing_anything_when_the_conversation_does_not_exist()
    {
        this.ShouldNotFindConversation();

        Func<Task> act = async () =>
            await this._handler.Handle(ConversationRemoverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        this.ShouldNotHaveRemovedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_as_not_found_without_removing_anything_when_the_conversation_belongs_to_another_user()
    {
        Conversation conversation = ConversationMother.Random();
        this.ShouldFindConversation(conversation);

        Func<Task> act = async () => await this._handler.Handle(
            ConversationRemoverCommandMother.With(conversation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        this.ShouldNotHaveRemovedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }
}
