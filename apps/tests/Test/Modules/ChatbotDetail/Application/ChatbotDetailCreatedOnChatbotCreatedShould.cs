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
public sealed class ChatbotDetailCreatedOnChatbotCreatedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailCreatedOnChatbotCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailCreatedOnChatbotCreated(this.DetailRepository.Object);
    }

    [Test]
    public async Task save_a_detail_with_the_data_of_the_chatbot_and_empty_collections()
    {
        ChatbotCreated @event = ChatbotCreatedMother.With(
            settings: ChatbotSettingsMother.With(socratic: true, explicaMas: false));

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedChatbotDetailMatching(detail =>
            detail.Id == @event.Id.Value
            && detail.Label == @event.Label
            && detail.Description == @event.Description
            && detail.Status == @event.Status.ToString()
            && detail.Socratic
            && !detail.ExplicaMas
            && detail.Provider == @event.Settings.Provider.ToString()
            && detail.Model == @event.Settings.Model
            && !detail.Archived
            && detail.IconUrl == null
            && detail.Docs.Count == 0
            && detail.Team.Count == 0
            && detail.Gaps.Count == 0
            && detail.TopQuestions.Count == 0
            && detail.Stats.Count == 0);
        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
