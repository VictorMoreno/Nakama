using Api.Configuration;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Finder;
using Nakama.FormationEnrollment.Application.ModuleCompleter;
using Nakama.FormationEnrollment.Application.TeamMemberAdder;
using Nakama.FormationEnrollment.Application.TeamMemberRemover;
using Nakama.Formations.Application.Archiver;
using Nakama.Formations.Application.Creator;
using Nakama.Formations.Application.IconUpdater;
using Nakama.Formations.Application.ModuleAdder;
using Nakama.Formations.Application.ModuleGatingToggler;
using Nakama.Formations.Application.ModuleRemover;
using Nakama.Formations.Application.ModuleUpdater;
using Nakama.Formations.Application.StatusUpdater;
using Nakama.FormationsList.Application;
using Nakama.FormationsList.Application.Searcher;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class FormationEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("formations").WithTags("Formations");

        group.MapPut("/{id:guid}", CreateFormation)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Creates a new formation with a caller-supplied id.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/archive", ArchiveFormation)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Marks a formation as archived.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", UpdateFormationStatus)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Updates a formation's status.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/icon", UploadFormationIcon)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Uploads or replaces a formation's icon image (PNG or JPEG, max 2 MB).")
            .DisableAntiforgery() // stateless JSON/file API, no cookie-based auth to forge against
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/icon", RemoveFormationIcon)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Removes a formation's icon image.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/modules/{moduleId:guid}", AddFormationModule)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Adds a module to a formation, using a caller-supplied module id.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}/modules/{moduleId:guid}", UpdateFormationModule)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Updates an existing module's fields.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/modules/{moduleId:guid}", RemoveFormationModule)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Removes a module from a formation's curriculum.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/modules/{moduleId:guid}/complete", CompleteFormationModule)
            .WithSummary("Marks a module complete, with an optional score.")
            .WithDescription(
                "Enforces sequential gating: fails if the previous module is gated and hasn't been completed yet.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/modules/{moduleId:guid}/toggle-gating", ToggleFormationModuleGating)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Flips a module's gated flag.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/team", AddFormationTeamMember)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Assigns a person to a formation - this is also their enrollment (fused with progress).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/team/{userId:guid}", RemoveFormationTeamMember)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary(
                "Unassigns a person from a formation - also deletes their progress (enrollment fused with team membership).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", SearchFormationSummaries)
            .WithSummary(
                "Lists formations (summary view), scoped to the caller: admins see every formation, anyone else only the ones they're assigned to.")
            .Produces<IReadOnlyCollection<FormationSummaryResponse>>();

        group.MapGet("/{id:guid}", FindFormationDetail)
            .WithSummary("Gets the full detail page projection for one formation.")
            .Produces<FormationDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/enrollment", FindFormationEnrollment)
            .WithSummary("Gets one user's own progress in a formation. 404 if they're not assigned.")
            .Produces<FormationEnrollmentResponse>()
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

    private static async Task<IResult> ArchiveFormation(Guid id, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new FormationArchiver.Command(id), ct);
        return Results.NoContent();
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

    private static async Task<IResult> UploadFormationIcon(
        Guid id,
        IFormFile file,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await using Stream stream = file.OpenReadStream();
        FormationIconUpdater.Upload upload = new FormationIconUpdater.Upload(file.ContentType, stream, file.Length);
        await commandBus.Send(new FormationIconUpdater.Command(id, upload), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFormationIcon(
        Guid id,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new FormationIconUpdater.Command(id, null), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AddFormationModule(
        Guid id, Guid moduleId, AddFormationModuleRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(
            new FormationModuleAdder.Command(
                id, moduleId, body.Title, body.Type, body.Summary, body.Gated, body.AiDraft,
                body.ResourceUrl, body.DurationMin,
                body.Quiz
                    ?.Select(q => new FormationModuleAdder.QuizQuestionInput(q.Question, q.Options, q.CorrectIndex))
                    .ToList(),
                body.PassingScore),
            ct);
        return Results.Created();
    }

    private static async Task<IResult> UpdateFormationModule(
        Guid id, Guid moduleId, UpdateFormationModuleRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(
            new FormationModuleUpdater.Command(
                id, moduleId, body.Title, body.Type, body.Summary, body.Gated, body.AiDraft,
                body.ResourceUrl, body.DurationMin,
                body.Quiz?.Select(q =>
                    new FormationModuleUpdater.QuizQuestionInput(q.Question, q.Options, q.CorrectIndex)).ToList(),
                body.PassingScore),
            ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFormationModule(
        Guid id, Guid moduleId, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new FormationModuleRemover.Command(id, moduleId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CompleteFormationModule(
        Guid id, Guid moduleId, CompleteFormationModuleRequest body, ICurrentUser currentUser, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new FormationModuleCompleter.Command(id, moduleId, currentUser.Id, body.Score), ct);
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

    private static async Task<IResult> AddFormationTeamMember(
        Guid id, AddFormationTeamMemberRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new FormationTeamMemberAdder.Command(id, body.UserId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFormationTeamMember(
        Guid id,
        Guid userId,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(new FormationTeamMemberRemover.Command(id, userId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SearchFormationSummaries(
        ICurrentUser currentUser,
        IQueryBus queryBus,
        CancellationToken ct)
    {
        IReadOnlyCollection<FormationSummaryResponse> response =
            await queryBus.Ask(new FormationSummarySearcher.Query(currentUser.Id, currentUser.IsAdmin), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindFormationDetail(Guid id, IQueryBus queryBus, CancellationToken ct)
    {
        FormationDetailResponse response = await queryBus.Ask(new FormationDetailFinder.Query(id), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> FindFormationEnrollment(Guid id,
        ICurrentUser currentUser,
        IQueryBus queryBus,
        CancellationToken ct)
    {
        FormationEnrollmentResponse response = await queryBus.Ask(new FormationEnrollmentFinder.Query(id, currentUser.Id), ct);
        return Results.Ok(response);
    }

    private sealed record CreateFormationRequest(
        string Title,
        string Audience,
        string Description,
        int Status,
        Guid? LinkedChatbotKey);

    private sealed record UpdateFormationStatusRequest(int Status);

    private sealed record QuizQuestionRequest(string Question, IReadOnlyList<string> Options, int CorrectIndex);

    private sealed record AddFormationModuleRequest(
        string Title,
        int Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestionRequest>? Quiz,
        int? PassingScore);

    private sealed record UpdateFormationModuleRequest(
        string Title,
        int Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestionRequest>? Quiz,
        int? PassingScore);

    private sealed record CompleteFormationModuleRequest(int? Score);

    private sealed record AddFormationTeamMemberRequest(Guid UserId);
}
