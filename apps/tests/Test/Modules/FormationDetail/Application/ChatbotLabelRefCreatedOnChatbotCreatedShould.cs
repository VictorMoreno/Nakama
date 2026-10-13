using Nakama.Chatbots.Domain.Events;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class ChatbotLabelRefCreatedOnChatbotCreatedShould : FormationDetailUnitTestCase
{
    private ChatbotLabelRefCreatedOnChatbotCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotLabelRefCreatedOnChatbotCreated(this.LabelRefsRepository.Object);
    }

    [Test]
    public async Task save_a_label_reference_with_the_id_and_label_of_the_chatbot()
    {
        ChatbotCreated @event = ChatbotCreatedMother.Random();

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedChatbotLabelRefMatching(labelRef =>
            labelRef.Id == @event.Id.Value && labelRef.Label == @event.Label);
    }
}
