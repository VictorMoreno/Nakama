using AwesomeAssertions;
using Nakama.Chatbots.Application.SettingsUpdater;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Nakama.LlmProviders.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotSettingsUpdaterShould : ChatbotsUnitTestCase
{
    private ChatbotSettingsUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSettingsUpdater.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task replace_the_settings_and_publish_chatbot_settings_updated()
    {
        Chatbot chatbot = ChatbotMother.With(settings: ChatbotSettingsMother.Random());
        ChatbotSettingsUpdater.Command command = ChatbotSettingsUpdaterCommandMother.With(chatbot.Id.Value);
        ChatbotSettings settings = new(
            command.Socratic,
            command.ExplicaMas,
            (LlmProvider)command.Provider,
            command.Model);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(command, CancellationToken.None);

        chatbot.Settings.Should().Be(settings);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotSettingsUpdated>(
            @event => @event.Id == chatbot.Id && @event.Settings == settings);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task fail_without_saving_anything_when_the_settings_have_no_model(string model)
    {
        Chatbot chatbot = ChatbotMother.With(settings: ChatbotSettingsMother.Random());
        ChatbotSettings previous = chatbot.Settings;
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotSettingsUpdaterCommandMother.With(chatbot.Id.Value, model: model), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotModelRequiredError>();
        chatbot.Settings.Should().Be(previous);
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotSettingsUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_is_archived()
    {
        Chatbot chatbot = ChatbotMother.Archived();
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotSettingsUpdaterCommandMother.With(chatbot.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotArchivedError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
