using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.Analytics.Application;
using Nakama.ChatbotDetail.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.ConversationHistory.Application;
using Nakama.ConversationHistory.Domain;
using Nakama.LlmProviders.Domain;
using Nakama.Shared.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots;
using Test.Modules.Chatbots.Application;
using Test.Modules.Shared;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class ChatbotConversationShould : ApiTestCase
{
    [Test]
    public async Task stream_the_answer_in_chunks_and_keep_the_conversation_in_the_history()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        this.Llm.Answer("Hel", "lo ", "there");
        Guid conversationId = Guid.NewGuid();
        string title = WordMother.Random();
        string question = WordMother.Random();

        HttpResponseMessage response = await this.Client.AskChatbot(chatbot.Id, conversationId, title, question, member);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/event-stream");
        IReadOnlyList<SseFrame> frames = ServerSentEvents.Parse(await response.Content.ReadAsStringAsync());
        frames.Chunks().Should().Equal("Hel", "lo ", "there");
        frames[^1].Event.Should().Be("done");
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, member))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();
        ConversationSummaryRow conversation = history!.Entries.Should().ContainSingle().Which;
        conversation.Id.Should().Be(conversationId);
        conversation.Title.Should().Be(title);
        HistoryExampleRow exchange = conversation.Exchanges.Should().ContainSingle().Which;
        exchange.Question.Should().Be(question);
        exchange.Answer.Should().Be("Hello there");
    }

    [Test]
    public async Task send_the_model_the_prompt_of_the_chatbot_and_the_question()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        string question = WordMother.Random();

        await this.Client.AskChatbot(chatbot.Id, Guid.NewGuid(), WordMother.Random(), question, member);

        LlmRequest request = this.Llm.Requests.Should().ContainSingle().Which;
        request.Model.Should().Be(chatbot.Model);
        request.SystemPrompt.Should().Contain(chatbot.Label).And.Contain(chatbot.Description);
        request.History.Should().ContainSingle().Which.Should().BeEquivalentTo(new LlmMessage("user", question, []));
    }

    [Test]
    public async Task continue_a_conversation_sending_the_previous_exchanges_to_the_model()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        Guid conversationId = Guid.NewGuid();
        string firstQuestion = WordMother.Random();
        string secondQuestion = WordMother.Random();
        this.Llm.Answer("First answer");
        await this.Client.AskChatbot(chatbot.Id, conversationId, WordMother.Random(), firstQuestion, member);
        this.Llm.Answer("Second answer");

        await this.Client.AskChatbot(chatbot.Id, conversationId, WordMother.Random(), secondQuestion, member);

        this.Llm.Requests.Should().HaveCount(2);
        this.Llm.Requests[1].History.Select(message => (message.Role, message.Content)).Should().Equal(
            ("user", firstQuestion),
            ("assistant", "First answer"),
            ("user", secondQuestion));
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, member))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();
        history!.Entries.Should().ContainSingle().Which.Exchanges.Select(exchange => exchange.Answer)
            .Should().Equal("First answer", "Second answer");
    }

    [Test]
    public async Task count_the_answer_in_the_analytics_and_remember_the_question_in_the_chatbot_detail()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        string question = WordMother.Random();

        await this.Client.AskChatbot(chatbot.Id, Guid.NewGuid(), WordMother.Random(), question, member);

        AnalyticsResponse? analytics = await (await this.Client.GetAnalytics(admin))
            .Content.ReadFromJsonAsync<AnalyticsResponse>();
        analytics!.AnsweredThisWeek.Should().Be(1);
        analytics.UsageByBot.Should().ContainSingle(row => row.BotKey == chatbot.Id).Which.Value.Should().Be(1);
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(chatbot.Id, admin))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.TopQuestions.Should().ContainSingle(row => row.Question == question).Which.Count.Should().Be(1);
    }

    [Test]
    public async Task send_the_attachments_to_the_model_and_list_them_in_the_history()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        AttachedFile notes = new("notes.txt", BytesMother.Random());

        await this.Client.AskChatbot(chatbot.Id, Guid.NewGuid(), WordMother.Random(), WordMother.Random(), member, notes);

        LlmDocument document = this.Llm.Requests.Should().ContainSingle().Which.History[^1].Documents!.Should().ContainSingle().Which;
        document.Name.Should().Be("notes.txt");
        document.Kind.Should().Be(LlmDocumentKind.PlainText);
        document.Content.Should().Equal(notes.Content);
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, member))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();
        history!.Entries.Single().Exchanges.Single().Attachments.Should().Equal("notes.txt");
    }

    [Test]
    public async Task report_a_failure_of_the_model_as_an_error_event_and_save_nothing()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);
        this.Llm.FailAfter(["partial"], new InvalidOperationException(WordMother.Random()));

        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id,
            Guid.NewGuid(),
            WordMother.Random(),
            WordMother.Random(),
            member);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<SseFrame> frames = ServerSentEvents.Parse(await response.Content.ReadAsStringAsync());
        frames.Chunks().Should().Equal("partial");
        frames[^1].Event.Should().Be("error");
        frames[^1].Data.Should().Contain("nakama.unexpected");
        frames.Should().NotContain(frame => frame.Event == "done");
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, member))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();
        history!.Entries.Should().BeEmpty();
    }

    [Test]
    public async Task answer_not_found_before_streaming_when_the_chatbot_does_not_exist()
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.AskChatbot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            WordMother.Random(),
            WordMother.Random(),
            admin);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.chatbot_detail.not_found");
        this.Llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task refuse_to_ask_a_chatbot_whose_provider_is_not_connected()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.OpenAi);
        await this.Client.CreateChatbot(chatbot, admin);
        string member = await this.JoinTeamOf(chatbot, admin);

        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id,
            Guid.NewGuid(),
            WordMother.Random(),
            WordMother.Random(),
            member);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.chatbots.provider_not_connected");
        this.Llm.Requests.Should().BeEmpty();
    }

    private async Task<string> JoinTeamOf(ChatbotCreator.Command chatbot, string adminToken)
    {
        SignedInUser member = await this.SignInAs(isAdmin: false);
        await this.Client.AddChatbotTeamMember(chatbot.Id, member.Id, TeamRole.Editor, adminToken);

        return member.Token;
    }
}
