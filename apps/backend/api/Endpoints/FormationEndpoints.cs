using Api.Configuration;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.Formations.Application.Creator;
using Nakama.Formations.Application.ModuleAdder;
using Nakama.Formations.Application.ModuleCompleter;
using Nakama.Formations.Application.ModuleGatingToggler;
using Nakama.Formations.Application.StatusUpdater;
using Nakama.Formations.Domain;
using Nakama.FormationsList.Application;
using Nakama.FormationsList.Application.Searcher;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class FormationEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("formations").WithTags("Formations");

        group.MapPut("/{id:guid}", CreateFormation)
            .WithSummary("Creates a new formation with a caller-supplied id.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}/status", UpdateFormationStatus)
            .WithSummary("Updates a formation's status.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/modules/{moduleId:guid}", AddFormationModule)
            .WithSummary("Adds a module to a formation, using a caller-supplied module id.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/modules/{moduleId:guid}/complete", CompleteFormationModule)
            .WithSummary("Marks a module complete, with an optional score.")
            .WithDescription(
                "Enforces sequential gating: fails if the previous module is gated and hasn't been completed yet.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/modules/{moduleId:guid}/toggle-gating", ToggleFormationModuleGating)
            .WithSummary("Flips a module's gated flag.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", SearchFormationSummaries)
            .WithSummary("Lists all formations (summary view).")
            .Produces<IReadOnlyCollection<FormationSummaryResponse>>();

        group.MapGet("/{id:guid}", FindFormationDetail)
            .WithSummary("Gets the full detail page projection for one formation.")
            .Produces<FormationDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateFormation(
        Guid id,
        CreateFormationRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(
            new FormationCreator.Command(id, body.Title, body.Audience, body.Description, body.Status,
                body.LinkedChatbotKey), ct);
        return Results.Created();
    }

    private static async Task<IResult> UpdateFormationStatus(
        Guid id,
        UpdateFormationStatusRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new FormationStatusUpdater.Command(id, body.Status), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddFormationModule(
        Guid id, Guid moduleId, AddFormationModuleRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(
            new FormationModuleAdder.Command(
                id, moduleId, body.Title, body.Type, body.Summary, body.Gated, body.AiDraft,
                body.ResourceUrl, body.DurationMin, body.Quiz, body.PassingScore),
            ct);
        return Results.Created();
    }

    private static async Task<IResult> CompleteFormationModule(
        Guid id, Guid moduleId, CompleteFormationModuleRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new FormationModuleCompleter.Command(id, moduleId, body.Score), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ToggleFormationModuleGating(Guid id,
        Guid moduleId,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new FormationModuleGatingToggler.Command(id, moduleId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SearchFormationSummaries(IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new FormationSummarySearcher.Query(), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindFormationDetail(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        var response = await queryBus.Ask(new FormationDetailFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private sealed record CreateFormationRequest(
        string Title,
        string Audience,
        string Description,
        FormationStatus Status,
        Guid? LinkedChatbotKey);

    private sealed record UpdateFormationStatusRequest(FormationStatus Status);

    private sealed record AddFormationModuleRequest(
        string Title,
        ModuleType Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestion>? Quiz,
        int? PassingScore);

    private sealed record CompleteFormationModuleRequest(int? Score);
}