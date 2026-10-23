using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Nakama.Analytics.Application;
using Nakama.AnswerFeedback.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.ConversationHistory.Application;
using Nakama.LlmProviders.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;
using Test.Modules.Shared;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class ChatbotFeedbackShould : ApiTestCase
{
    [Test]
    public async Task keep_the_rating_of_an_answer_so_it_is_still_there_when_the_chat_is_loaded_again()
    {
        (ChatbotCreator.Command chatbot, string admin, SignedInUser member) = await this.CreateChatbotWithMember();
        Guid conversationId = await this.Ask(chatbot, member.Token);

        HttpResponseMessage rating = await this.Client.RateExchange(chatbot.Id, conversationId, 0, false, member.Token);

        rating.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.ReadExchangeRatings(chatbot, member.Token)).Should().Equal((bool?)false);
        await this.Client.RateExchange(chatbot.Id, conversationId, 0, true, member.Token);
        (await this.ReadExchangeRatings(chatbot, member.Token)).Should().Equal((bool?)true);
        _ = admin;
    }

    [Test]
    public async Task leave_unrated_the_answers_nobody_rated()
    {
        (ChatbotCreator.Command chatbot, _, SignedInUser member) = await this.CreateChatbotWithMember();
        await this.Ask(chatbot, member.Token);

        (await this.ReadExchangeRatings(chatbot, member.Token)).Should().Equal((bool?)null);
    }

    [Test]
    public async Task not_let_another_person_rate_an_answer_of_a_conversation_that_is_not_theirs()
    {
        (ChatbotCreator.Command chatbot, string admin, SignedInUser owner) = await this.CreateChatbotWithMember();
        SignedInUser other = await this.SignInAs(isAdmin: false);
        await this.Client.AddChatbotTeamMember(chatbot.Id, other.Id, TeamRole.Editor, admin);
        Guid conversationId = await this.Ask(chatbot, owner.Token);

        HttpResponseMessage rating = await this.Client.RateExchange(chatbot.Id, conversationId, 0, false, other.Token);

        rating.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await this.ReadExchangeRatings(chatbot, owner.Token)).Should().Equal((bool?)null);
    }

    [Test]
    public async Task answer_not_found_when_the_exchange_does_not_exist()
    {
        (ChatbotCreator.Command chatbot, _, SignedInUser member) = await this.CreateChatbotWithMember();
        Guid conversationId = await this.Ask(chatbot, member.Token);

        HttpResponseMessage rating = await this.Client.RateExchange(chatbot.Id, conversationId, 3, false, member.Token);

        rating.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task refuse_to_rate_to_someone_outside_the_team_of_the_chatbot()
    {
        (ChatbotCreator.Command chatbot, _, SignedInUser member) = await this.CreateChatbotWithMember();
        Guid conversationId = await this.Ask(chatbot, member.Token);
        string outsider = await this.SignIn(isAdmin: false);

        HttpResponseMessage rating = await this.Client.RateExchange(chatbot.Id, conversationId, 0, false, outsider);

        rating.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task list_the_questions_rated_as_not_useful_this_week_in_the_analytics()
    {
        (ChatbotCreator.Command chatbot, string admin, SignedInUser member) = await this.CreateChatbotWithMember();
        string question = WordMother.Random();
        Guid first = await this.Ask(chatbot, member.Token, question);
        Guid second = await this.Ask(chatbot, member.Token, question);
        await this.Client.RateExchange(chatbot.Id, first, 0, false, member.Token);
        await this.Client.RateExchange(chatbot.Id, second, 0, false, member.Token);

        UnhelpfulQuestionsResponse? response = await (await this.Client.Get("/api/analytics/unhelpful-questions", admin))
            .Content.ReadFromJsonAsync<UnhelpfulQuestionsResponse>();

        response!.Questions.Should().ContainSingle().Which.Should().Be(new UnhelpfulQuestionRow(chatbot.Id, question, 2));
    }

    [Test]
    public async Task stop_listing_a_question_when_its_rating_is_changed_to_useful()
    {
        (ChatbotCreator.Command chatbot, string admin, SignedInUser member) = await this.CreateChatbotWithMember();
        Guid conversationId = await this.Ask(chatbot, member.Token);
        await this.Client.RateExchange(chatbot.Id, conversationId, 0, false, member.Token);
        await this.Client.RateExchange(chatbot.Id, conversationId, 0, true, member.Token);

        UnhelpfulQuestionsResponse? response = await (await this.Client.Get("/api/analytics/unhelpful-questions", admin))
            .Content.ReadFromJsonAsync<UnhelpfulQuestionsResponse>();

        response!.Questions.Should().BeEmpty();
    }

    [Test]
    public async Task count_each_rated_answer_once_in_the_useful_rate_even_when_the_rating_changes()
    {
        (ChatbotCreator.Command chatbot, string admin, SignedInUser member) = await this.CreateChatbotWithMember();
        Guid first = await this.Ask(chatbot, member.Token);
        Guid second = await this.Ask(chatbot, member.Token);
        await this.Client.RateExchange(chatbot.Id, first, 0, false, member.Token);
        await this.Client.RateExchange(chatbot.Id, first, 0, true, member.Token);
        await this.Client.RateExchange(chatbot.Id, second, 0, false, member.Token);

        AnalyticsResponse? analytics = await (await this.Client.GetAnalytics(admin))
            .Content.ReadFromJsonAsync<AnalyticsResponse>();

        analytics!.UsefulFeedbackCount.Should().Be(1);
        analytics.TotalFeedbackCount.Should().Be(2);
    }

    [Test]
    public async Task refuse_the_unhelpful_questions_to_regular_users_and_to_anonymous_callers()
    {
        string member = await this.SignIn(isAdmin: false);

        (await this.Client.Get("/api/analytics/unhelpful-questions", member)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.Get("/api/analytics/unhelpful-questions", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(ChatbotCreator.Command Chatbot, string Admin, SignedInUser Member)> CreateChatbotWithMember()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        await this.Client.CreateChatbot(chatbot, admin);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        await this.Client.AddChatbotTeamMember(chatbot.Id, member.Id, TeamRole.Editor, admin);

        return (chatbot, admin, member);
    }

    private async Task<Guid> Ask(ChatbotCreator.Command chatbot, string accessToken, string? question = null)
    {
        this.Llm.Answer("ok");
        Guid conversationId = Guid.NewGuid();
        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id, conversationId, WordMother.Random(), question ?? WordMother.Random(), accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await response.Content.ReadAsStringAsync();

        return conversationId;
    }

    private async Task<List<bool?>> ReadExchangeRatings(ChatbotCreator.Command chatbot, string accessToken)
    {
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, accessToken))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();

        return history!.Entries.SelectMany(entry => entry.Exchanges).Select(exchange => exchange.Useful).ToList();
    }
}
