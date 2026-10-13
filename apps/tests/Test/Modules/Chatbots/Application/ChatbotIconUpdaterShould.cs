using AwesomeAssertions;
using Nakama.Chatbots.Application.IconUpdater;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotIconUpdaterShould : ChatbotsUnitTestCase
{
    private ChatbotIconUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotIconUpdater.Handler(
            new(this.ChatbotsRepository.Object, this.IconStorage.Object, this.EventBus.Object));
    }

    [Test]
    public async Task store_the_uploaded_icon_and_publish_chatbot_icon_updated()
    {
        string iconFileName = $"{WordMother.Random()}.png";
        Chatbot chatbot = ChatbotMother.Random();
        ChatbotIconUpdater.Command command = ChatbotIconUpdaterCommandMother.WithUpload(chatbot.Id.Value, "image/png");
        this.ShouldFindChatbot(chatbot);
        this.ShouldStoreIcon(iconFileName);

        await this._handler.Handle(command, CancellationToken.None);

        chatbot.IconUrl.Should().Be(iconFileName);
        this.ShouldHaveStoredIcon(chatbot.Id.Value, "image/png");
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotIconUpdated>(
            @event => @event.Id == chatbot.Id && @event.IconUrl == iconFileName);
    }

    [Test]
    public async Task delete_the_existing_icon_when_no_file_is_uploaded()
    {
        Chatbot chatbot = ChatbotMother.With(iconUrl: $"{WordMother.Random()}.png");
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(ChatbotIconUpdaterCommandMother.WithoutUpload(chatbot.Id.Value), CancellationToken.None);

        chatbot.IconUrl.Should().BeNull();
        this.ShouldHaveDeletedExistingIcon(chatbot.Id.Value);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotIconUpdated>(@event => @event.Id == chatbot.Id && @event.IconUrl is null);
    }

    [Test]
    public async Task fail_without_touching_the_storage_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotIconUpdaterCommandMother.WithUpload(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveTouchedTheIconStorage();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
