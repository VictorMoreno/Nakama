using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotSummaries.Domain;
using Nakama.ChatbotSummaries.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationHistory.Application;
using Nakama.LlmProviders.Domain;
using Nakama.Shared.Infrastructure.Events;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;
using Test.Modules.Shared;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class UnitOfWorkShould : ApiTestCase
{
    private readonly FailureSwitch _failure = new();

    [SetUp]
    public void ResetFailures() => this._failure.Reset();

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(this._failure);
        services.AddTransient<INotificationHandler<DomainEventNotification<ChatbotCreated>>, FailAfterChatbotCreated>();
        services.AddTransient<INotificationHandler<DomainEventNotification<ConversationExchangeAppended>>, FailAfterExchangeAppended>();
        services.AddTransient<INotificationHandler<DomainEventNotification<ConversationStarted>>, ViolateConstraintOnConversationStarted>();
    }

    [Test]
    public async Task roll_back_the_chatbot_and_every_projection_when_a_projection_handler_fails()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.Random();
        this._failure.Arm();

        HttpResponseMessage created = await this.Client.CreateChatbot(chatbot, admin);

        created.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(admin))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        chatbots.Should().BeEmpty();
        (await this.Client.GetChatbot(chatbot.Id, admin)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task keep_the_chatbot_when_everything_goes_well_and_all_its_projections_with_it()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.Random();

        HttpResponseMessage created = await this.Client.CreateChatbot(chatbot, admin);

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(admin))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        chatbots.Should().ContainSingle(item => item.Id == chatbot.Id);
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(chatbot.Id, admin))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.Label.Should().Be(chatbot.Label);
    }

    [Test]
    public async Task report_an_error_event_and_save_no_conversation_when_a_projection_handler_fails_while_asking()
    {
        (ChatbotCreator.Command chatbot, string member) = await this.ChatbotWithAMember();
        this.Llm.Answer("Hel", "lo");
        this._failure.Arm();

        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id,
            Guid.NewGuid(),
            WordMother.Random(),
            WordMother.Random(),
            member);

        IReadOnlyList<SseFrame> frames = ServerSentEvents.Parse(await response.Content.ReadAsStringAsync());
        frames[^1].Event.Should().Be("error");
        frames.Should().NotContain(frame => frame.Event == "done");
        await this.ShouldHaveNoConversations(chatbot, member);
    }

    [Test]
    public async Task report_an_error_event_instead_of_done_when_the_commit_of_the_answer_fails()
    {
        (ChatbotCreator.Command chatbot, string member) = await this.ChatbotWithAMember();
        this.Llm.Answer("Hel", "lo");
        this._failure.ArmConstraintViolation();

        HttpResponseMessage response = await this.Client.AskChatbot(
            chatbot.Id,
            Guid.NewGuid(),
            WordMother.Random(),
            WordMother.Random(),
            member);

        IReadOnlyList<SseFrame> frames = ServerSentEvents.Parse(await response.Content.ReadAsStringAsync());
        frames.Chunks().Should().Equal("Hel", "lo");
        frames[^1].Event.Should().Be("error");
        frames.Should().NotContain(frame => frame.Event == "done");
        await this.ShouldHaveNoConversations(chatbot, member);
    }

    private async Task<(ChatbotCreator.Command Chatbot, string MemberToken)> ChatbotWithAMember()
    {
        string admin = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command chatbot = ChatbotCreatorCommandMother.With(provider: LlmProvider.Anthropic);
        (await this.Client.CreateChatbot(chatbot, admin)).StatusCode.Should().Be(HttpStatusCode.Created);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        (await this.Client.AddChatbotTeamMember(chatbot.Id, member.Id, TeamRole.Reader, admin))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        return (chatbot, member.Token);
    }

    private async Task ShouldHaveNoConversations(ChatbotCreator.Command chatbot, string member)
    {
        ConversationHistoryResponse? history = await (await this.Client.GetConversationHistory(chatbot.Id, member))
            .Content.ReadFromJsonAsync<ConversationHistoryResponse>();

        history.Should().NotBeNull();
        history.Entries.Should().BeEmpty();
    }

    private sealed class FailureSwitch
    {
        public bool Failing { get; private set; }

        public bool ViolatingConstraint { get; private set; }

        public void Arm() => this.Failing = true;

        public void Reset()
        {
            this.Failing = false;
            this.ViolatingConstraint = false;
        }

        public void ArmConstraintViolation() => this.ViolatingConstraint = true;
    }

    private sealed class FailAfterChatbotCreated(FailureSwitch failure)
        : INotificationHandler<DomainEventNotification<ChatbotCreated>>
    {
        public Task Handle(DomainEventNotification<ChatbotCreated> notification, CancellationToken cancellationToken) =>
            failure.Failing ? throw new InvalidOperationException("projection failed") : Task.CompletedTask;
    }

    private sealed class FailAfterExchangeAppended(FailureSwitch failure)
        : INotificationHandler<DomainEventNotification<ConversationExchangeAppended>>
    {
        public Task Handle(DomainEventNotification<ConversationExchangeAppended> notification, CancellationToken cancellationToken) =>
            failure.Failing ? throw new InvalidOperationException("projection failed") : Task.CompletedTask;
    }

    private sealed class ViolateConstraintOnConversationStarted(FailureSwitch failure, IChatbotSummaryRepository summaries)
        : INotificationHandler<DomainEventNotification<ConversationStarted>>
    {
        public Task Handle(DomainEventNotification<ConversationStarted> notification, CancellationToken cancellationToken) =>
            failure.ViolatingConstraint
                ? summaries.Save(ChatbotSummary.Create(Guid.NewGuid(), new string('x', 300), "description", "Active"), cancellationToken)
                : Task.CompletedTask;
    }
}
