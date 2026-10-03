using Api.Configuration;
using Nakama.LlmProviders.Application;
using Nakama.LlmProviders.Application.Registerer;
using Nakama.LlmProviders.Application.Searcher;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class LlmProviderEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("llm-providers").WithTags("LlmProviders");

        group.MapGet("/", SearchLlmProviders)
            .WithSummary("Lists every known LLM provider with its configuration status and model catalog.")
            .WithDescription("Never returns the stored API key - only whether one is set.")
            .Produces<IReadOnlyList<LlmProviderConfigResponse>>();

        group.MapPut("/{provider:int}", RegisterLlmProvider)
            .WithSummary("Registers or replaces a provider's API key, enabled flag, and model catalog.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> SearchLlmProviders(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new LlmProviderSearcher.Query(), ct));

    private static async Task<IResult> RegisterLlmProvider(
        int provider,
        RegisterLlmProviderRequest body,
        ICommandBus commandBus,
        CancellationToken ct)
    {
        await commandBus.Send(
            new LlmProviderRegisterer.Command(
                provider,
                body.ApiKey,
                body.Enabled,
                body.Models.Select(m => new LlmProviderRegisterer.ModelInput(m.Id, m.Label)).ToList()),
            ct);
        return Results.NoContent();
    }

    private sealed record RegisterLlmProviderRequest(string ApiKey, bool Enabled, IReadOnlyList<ModelInput> Models);

    private sealed record ModelInput(string Id, string Label);
}
