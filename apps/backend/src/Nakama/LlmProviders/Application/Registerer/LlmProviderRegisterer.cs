using Nakama.LlmProviders.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Application.Registerer;

public static class LlmProviderRegisterer
{
    public sealed record ModelInput(string Id, string Label);

    public sealed record Command(int Provider, string ApiKey, bool Enabled, IReadOnlyList<ModelInput> Models) : ICommand;

    public sealed class Handler(Registerer registerer) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await registerer.Execute(
                (LlmProvider)cmd.Provider,
                cmd.ApiKey,
                cmd.Enabled,
                cmd.Models.Select(m => new LlmModelOption(m.Id, m.Label)).ToList(),
                ct);
        }
    }

    public sealed class Registerer(ILlmProviderConfigRepository repository, ISystemClock systemClock) : IService
    {
        public async Task Execute(LlmProvider provider, string apiKey, bool enabled, IReadOnlyList<LlmModelOption> models,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LlmProviderConfig> found =
                await repository.Search(new LlmProviderConfigByProviderSpecification(provider), cancellationToken);
            LlmProviderConfig? existing = found.FirstOrDefault();

            if (existing is null)
            {
                LlmProviderConfig config = LlmProviderConfig.Register(provider, apiKey, enabled, models, systemClock);
                await repository.Save(config, cancellationToken);
                return;
            }

            string nextApiKey = string.IsNullOrEmpty(apiKey) ? existing.ApiKey : apiKey;
            existing.Update(nextApiKey, enabled, models, systemClock);
            await repository.Save(existing, cancellationToken);
        }
    }
}
