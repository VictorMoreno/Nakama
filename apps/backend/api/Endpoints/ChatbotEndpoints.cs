using System.Text.Json;
using Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.Chatbots.Application.Archiver;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Application.DocumentAdder;
using Nakama.Chatbots.Application.DocumentRemover;
using Nakama.Chatbots.Application.IconUpdater;
using Nakama.Chatbots.Application.QuestionAsker;
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
        RouteGroupBuilder group = app.MapGroup("chatbots").WithTags("Chatbots");

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

        group.MapPost("/{botKey:guid}/conversations/{conversationId:guid}/ask", AskChatbot)
            .WithSummary("Streams a chatbot answer token-by-token via Server-Sent Events.")
            .WithDescription(
                "Generates the answer with the chatbot's configured LLM provider/model and persists the exchange once " +
                "the stream completes. If no conversation exists yet with the given conversationId, one is started " +
                "implicitly using that exact id. The request is multipart/form-data: userId, title, question and up to 5 " +
                "optional PDF/Markdown/text attachments in `files`. Preflight failures (chatbot not found, provider not connected) still " +
                "surface as a normal 404/422 JSON response; failures once streaming has started surface as an " +
                "`event: error` SSE frame instead, since the HTTP status is already committed by then.")
            .DisableAntiforgery()
            .ExcludeFromDescription();

        group.MapDelete("/conversations/{conversationId:guid}", DeleteConversation)
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
            .WithSummary("Gets the chat UI projection for one chatbot (opening example).")
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
                body.Provider,
                body.Model), ct);
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
        await commandBus.Send(new ChatbotSettingsUpdater.Command(id, body.Socratic, body.ExplicaMas, body.Provider, body.Model),
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
        CancellationToken ct)
    {
        await using Stream stream = file.OpenReadStream();
        await commandBus.Send(new ChatbotDocumentAdder.Command(id, documentId, name, origin, file.FileName, stream, file.Length), ct);
        return Results.Created();
    }

    private static async Task<IResult> RemoveChatbotDocument(
        Guid id,
        int index,
        ICommandBus commandBus,
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
        CancellationToken ct)
    {
        await using Stream stream = file.OpenReadStream();
        ChatbotIconUpdater.Upload upload = new(file.ContentType, stream, file.Length);
        await commandBus.Send(new ChatbotIconUpdater.Command(id, upload), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveChatbotIcon(
        Guid id,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new ChatbotIconUpdater.Command(id, null), ct);
        return Results.NoContent();
    }

    private static async Task AskChatbot(
        Guid botKey,
        Guid conversationId,
        [FromForm] Guid userId,
        [FromForm] string title,
        [FromForm] string question,
        IFormFileCollection files,
        ChatbotQuestionAsker.Asker asker,
        HttpContext httpContext,
        CancellationToken ct)
    {
        List<ChatbotQuestionAsker.AttachmentUpload> attachments = await ReadAttachments(files, ct);

        ChatbotQuestionAsker.Command cmd = new(botKey, conversationId, userId, title, question, attachments);
        IAsyncEnumerator<string> stream = asker.Execute(cmd, ct).GetAsyncEnumerator(ct);
        bool hasChunk = await stream.MoveNextAsync();

        httpContext.Response.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            while (hasChunk)
            {
                await WriteSseEvent(httpContext.Response, "chunk", JsonSerializer.Serialize(stream.Current), ct);
                hasChunk = await stream.MoveNextAsync();
            }

            await WriteSseEvent(httpContext.Response, "done", "{}", ct);
        }
        catch (Exception ex)
        {
            (string? code, string? detail) = ex is DomainError domainError
                ? (domainError.Code, domainError.Message)
                : ("nakama.unexpected", "No se pudo completar la respuesta.");
            await WriteSseEvent(httpContext.Response, "error", JsonSerializer.Serialize(new { code, detail }), ct);
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    private static async Task<List<ChatbotQuestionAsker.AttachmentUpload>> ReadAttachments(IFormFileCollection files, CancellationToken ct)
    {
        List<ChatbotQuestionAsker.AttachmentUpload> attachments = new(files.Count);
        foreach (IFormFile file in files)
        {
            await using Stream fileStream = file.OpenReadStream();
            using MemoryStream buffer = new();
            await fileStream.CopyToAsync(buffer, ct);
            attachments.Add(new ChatbotQuestionAsker.AttachmentUpload(file.FileName, buffer.ToArray()));
        }

        return attachments;
    }

    private static async Task WriteSseEvent(HttpResponse response, string eventName, string jsonData, CancellationToken ct)
    {
        await response.WriteAsync($"event: {eventName}\ndata: {jsonData}\n\n", ct);
        await response.Body.FlushAsync(ct);
    }

    private static async Task<IResult> DeleteConversation(
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
        IReadOnlyCollection<ChatbotSummaryResponse> response = await queryBus.Ask(new ChatbotSummarySearcher.Query(), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindChatbotDetail(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        ChatbotDetailResponse response = await queryBus.Ask(new ChatbotDetailFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindChatPage(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        ChatPageResponse response = await queryBus.Ask(new ChatPageFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindConversationHistory(Guid botKey, Guid? userId, IQueryBus queryBus, CancellationToken ct)
    {
        ConversationHistoryResponse response = await queryBus.Ask(new ConversationHistoryFinder.Query(botKey, userId), ct);
        return Results.Ok(response);
    }

    private sealed record CreateChatbotRequest(
        string Label,
        string Description,
        int Status,
        bool General,
        bool Socratic,
        bool ExplicaMas,
        int Provider,
        string Model);

    private sealed record UpdateChatbotStatusRequest(int Status);

    private sealed record UpdateChatbotSettingsRequest(bool Socratic, bool ExplicaMas, int Provider, string Model);

    private sealed record AddChatbotTeamMemberRequest(Guid UserId, int Role);
}
