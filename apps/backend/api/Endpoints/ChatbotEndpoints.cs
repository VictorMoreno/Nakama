using Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.Chatbots.Application.Archiver;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Application.DocumentAdder;
using Nakama.Chatbots.Application.DocumentRemover;
using Nakama.Chatbots.Application.ExchangeAppender;
using Nakama.Chatbots.Application.IconUpdater;
using Nakama.Chatbots.Application.Remover;
using Nakama.Chatbots.Application.SettingsUpdater;
using Nakama.Chatbots.Application.StatusUpdater;
using Nakama.Chatbots.Application.TeamMemberAdder;
using Nakama.Chatbots.Application.TeamMemberRemover;
using Nakama.ChatbotSummaries.Application;
using Nakama.ChatbotSummaries.Application.Searcher;
using Nakama.ChatPage.Application;
using Nakama.ChatPage.Application.Finder;
using Nakama.ConversationHistory.Application;
using Nakama.ConversationHistory.Application.Finder;
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

        group.MapPut("/{id:guid}/documents/{documentId:guid}", AddChatbotDocument)
            .WithSummary("Uploads a document (PDF, Markdown, or plain text, max 20 MB) to a chatbot's knowledge base.")
            .WithDescription(
                "Newly uploaded documents are always reported as Pending - this codebase has no real indexing pipeline yet.")
            .DisableAntiforgery() // stateless JSON/file API, no cookie-based auth to forge against
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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

        group.MapPost("/{id:guid}/icon", UploadChatbotIcon)
            .WithSummary("Uploads or replaces a chatbot's icon image (PNG or JPEG, max 2 MB).")
            .DisableAntiforgery() // stateless JSON/file API, no cookie-based auth to forge against
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/icon", RemoveChatbotIcon)
            .WithSummary("Removes a chatbot's icon image.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{botKey:guid}/conversations/{conversationId:guid}/exchanges", AppendConversationExchange)
            .WithSummary("Appends a question/answer exchange to a conversation.")
            .WithDescription(
                "If no conversation exists yet with the given conversationId, one is started implicitly using that exact id - this endpoint covers both 'first message in a new chat' and 'reply within an existing conversation'.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{botKey:guid}/conversations/{conversationId:guid}", DeleteConversation)
            .WithSummary("Deletes a user's own conversation (hard delete). The immutable conversation audit log is untouched.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", SearchChatbotSummaries)
            .WithSummary("Lists all chatbots (summary view).")
            .Produces<IReadOnlyCollection<ChatbotSummaryResponse>>();

        group.MapGet("/{id:guid}", FindChatbotDetail)
            .WithSummary("Gets the full detail page projection for one chatbot.")
            .Produces<ChatbotDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/chat-page", FindChatPage)
            .WithSummary("Gets the chat UI projection for one chatbot (opening example and chips).")
            .Produces<ChatPageResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{botKey:guid}/conversation-history", FindConversationHistory)
            .WithSummary("Gets a user's conversation history for one chatbot.")
            .Produces<ConversationHistoryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateChatbot(
        Guid id,
        CreateChatbotRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(
            new ChatbotCreator.Command(id,
                body.Label,
                body.Description,
                body.Status,
                body.General,
                body.Socratic,
                body.ExplicaMas,
                body.Provider), ct);
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
        UpdateChatbotSettingsRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotSettingsUpdater.Command(id, body.Socratic, body.ExplicaMas, body.Provider),
            ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddChatbotDocument(
        Guid id,
        Guid documentId,
        IFormFile file,
        [FromForm] string name,
        [FromForm] string origin,
        ICommandBus commandBus,
        IConfiguration configuration,
        IWebHostEnvironment env,
        CancellationToken ct)
    {
        var basePath = DocumentStorage.ResolvePath(configuration, env, "ChatbotDocumentsPath");
        var fileName = await DocumentStorage.SaveAsync(basePath, documentId, file, ct);
        await commandBus.Send(new ChatbotDocumentAdder.Command(id, documentId, name, origin, fileName), ct);
        return Results.Created();
    }

    private static async Task<IResult> RemoveChatbotDocument(
        Guid id,
        int index,
        ICommandBus commandBus,
        IQueryBus queryBus,
        IConfiguration configuration,
        IWebHostEnvironment env,
        CancellationToken ct)
    {
        var detail = await queryBus.Ask(new ChatbotDetailFinder.Query(id), ct);
        var fileName = index >= 0 && index < detail.Docs.Count ? Path.GetFileName(detail.Docs[index].Url) : null;

        await commandBus.Send(new ChatbotDocumentRemover.Command(id, index), ct);

        if (fileName is not null)
        {
            var basePath = DocumentStorage.ResolvePath(configuration, env, "ChatbotDocumentsPath");
            DocumentStorage.Delete(basePath, fileName);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> AddChatbotTeamMember(
        Guid id,
        AddChatbotTeamMemberRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotTeamMemberAdder.Command(id, body.UserId, body.Role), ct);
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

    private static async Task<IResult> UploadChatbotIcon(
        Guid id,
        IFormFile file,
        ICommandBus commandBus,
        IConfiguration configuration,
        IWebHostEnvironment env,
        CancellationToken ct)
    {
        var basePath = IconStorage.ResolvePath(configuration, env, "ChatbotIconsPath");
        var fileName = await IconStorage.SaveAsync(basePath, id, file, ct);
        await commandBus.Send(new ChatbotIconUpdater.Command(id, fileName), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveChatbotIcon(
        Guid id,
        ICommandBus commandBus,
        IConfiguration configuration,
        IWebHostEnvironment env,
        CancellationToken ct)
    {
        var basePath = IconStorage.ResolvePath(configuration, env, "ChatbotIconsPath");
        IconStorage.DeleteExisting(basePath, id);
        await commandBus.Send(new ChatbotIconUpdater.Command(id, null), ct);
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
                conversationId, botKey, body.UserId, body.Title, body.Question, body.Answer, body.Citation,
                body.MoreExplanation, body.SocraticVariant),
            ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteConversation(
        Guid botKey,
        Guid conversationId,
        Guid userId,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ConversationRemover.Command(conversationId, userId), ct);
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

    private static async Task<IResult> FindConversationHistory(Guid botKey, Guid? userId, IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new ConversationHistoryFinder.Query(botKey, userId), ct);
        return Results.Ok(response);
    }

    private sealed record CreateChatbotRequest(
        string Label,
        string Description,
        int Status,
        bool General,
        bool Socratic,
        bool ExplicaMas,
        int Provider);

    private sealed record UpdateChatbotStatusRequest(int Status);

    private sealed record UpdateChatbotSettingsRequest(bool Socratic, bool ExplicaMas, int Provider);

    private sealed record AddChatbotTeamMemberRequest(Guid UserId, int Role);

    private sealed record AppendConversationExchangeRequest(
        Guid UserId,
        string Title,
        string Question,
        string Answer,
        string Citation,
        string MoreExplanation,
        string SocraticVariant);
}