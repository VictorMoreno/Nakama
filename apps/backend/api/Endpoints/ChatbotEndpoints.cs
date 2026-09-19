using Api.Configuration;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.Chatbots.Application.Archiver;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Application.DocumentAdder;
using Nakama.Chatbots.Application.DocumentRemover;
using Nakama.Chatbots.Application.ExchangeAppender;
using Nakama.Chatbots.Application.SettingsUpdater;
using Nakama.Chatbots.Application.StatusUpdater;
using Nakama.Chatbots.Application.TeamMemberAdder;
using Nakama.Chatbots.Application.TeamMemberRemover;
using Nakama.Chatbots.Domain;
using Nakama.ChatbotsList.Application;
using Nakama.ChatbotsList.Application.Searcher;
using Nakama.ChatPage.Application;
using Nakama.ChatPage.Application.Finder;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class ChatbotEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("chatbots").WithTags("Chatbots");

        group.MapPut("/{id:guid}", CreateChatbot)
            .WithSummary("Creates a new chatbot with a caller-supplied id.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/archive", ArchiveChatbot)
            .WithSummary("Marks a chatbot as archived.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", UpdateChatbotStatus)
            .WithSummary("Updates a chatbot's status.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/settings", UpdateChatbotSettings)
            .WithSummary("Replaces a chatbot's settings (Socratic mode, ExplicaMas, LLM provider).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/documents", AddChatbotDocument)
            .WithSummary("Adds a document to a chatbot's knowledge base.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/documents/{index:int}", RemoveChatbotDocument)
            .WithSummary("Removes a document by its list position.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/team", AddChatbotTeamMember)
            .WithSummary("Adds a team member to a chatbot.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/team/{index:int}", RemoveChatbotTeamMember)
            .WithSummary("Removes a team member by their list position. The owner cannot be removed.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{botKey:guid}/conversations/{conversationId:guid}/exchanges", AppendConversationExchange)
            .WithSummary("Appends a question/answer exchange to a conversation.")
            .WithDescription(
                "If no conversation exists yet with the given conversationId, one is started implicitly using that exact id - this endpoint covers both 'first message in a new chat' and 'reply within an existing conversation'.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/", SearchChatbotSummaries)
            .WithSummary("Lists all chatbots (summary view).")
            .Produces<IReadOnlyCollection<ChatbotSummaryResponse>>();

        group.MapGet("/{id:guid}", FindChatbotDetail)
            .WithSummary("Gets the full detail page projection for one chatbot.")
            .Produces<ChatbotDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/chat-page", FindChatPage)
            .WithSummary("Gets the chat UI projection for one chatbot (opening example, chips, conversation history).")
            .Produces<ChatPageResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateChatbot(
        Guid id,
        CreateChatbotRequest body,
        ICommandBus commandBus, 
        CancellationToken ct)
    {
        await commandBus.Send(
            new ChatbotCreator.Command(id, body.Label, body.Description, body.Status, body.General, body.Settings), ct);
        return Results.Created();
    }

    private static async Task<IResult> ArchiveChatbot(Guid id, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new ChatbotArchiver.Command(id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateChatbotStatus(
        Guid id, 
        UpdateChatbotStatusRequest body, 
        ICommandBus commandBus, 
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotStatusUpdater.Command(id, body.Status), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateChatbotSettings(
        Guid id,
        ChatbotSettings body, 
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotSettingsUpdater.Command(id, body), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddChatbotDocument(
        Guid id,
        AddChatbotDocumentRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(
            new ChatbotDocumentAdder.Command(id, body.Name, body.Origin, body.Status, body.StatusLabel), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveChatbotDocument(Guid id, int index, ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotDocumentRemover.Command(id, index), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddChatbotTeamMember(
        Guid id, 
        AddChatbotTeamMemberRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotTeamMemberAdder.Command(id, body.Name, body.Role), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveChatbotTeamMember(Guid id, 
        int index,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotTeamMemberRemover.Command(id, index), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AppendConversationExchange(
        Guid botKey, 
        Guid conversationId,
        AppendConversationExchangeRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(
            new ConversationExchangeAppender.Command(
                conversationId, botKey, body.Title, body.Question, body.Answer, body.Citation, body.MoreExplanation,
                body.SocraticVariant),
            ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SearchChatbotSummaries(IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new ChatbotSummarySearcher.Query(), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindChatbotDetail(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new ChatbotDetailFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindChatPage(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new ChatPageFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private sealed record CreateChatbotRequest(
        string Label,
        string Description,
        ChatbotStatus Status,
        bool General,
        ChatbotSettings Settings);

    private sealed record UpdateChatbotStatusRequest(ChatbotStatus Status);

    private sealed record AddChatbotDocumentRequest(string Name, string Origin, DocStatus Status, string StatusLabel);

    private sealed record AddChatbotTeamMemberRequest(string Name, TeamRole Role);

    private sealed record AppendConversationExchangeRequest(
        string Title,
        string Question,
        string Answer,
        string Citation,
        string MoreExplanation,
        string SocraticVariant);
}