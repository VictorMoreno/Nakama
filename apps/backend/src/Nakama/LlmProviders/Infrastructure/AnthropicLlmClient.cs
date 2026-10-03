using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Nakama.LlmProviders.Domain;
using Nakama.LlmProviders.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class AnthropicLlmClient(ILlmProviderConfigRepository repository) : ILlmClient
{
    public async Task<string> Ask(string model, string systemPrompt, IReadOnlyList<LlmMessage> history, CancellationToken ct)
    {
        var config = await repository.FindByProvider(LlmProvider.Anthropic, ct);
        if (config is null || !config.Enabled || string.IsNullOrEmpty(config.ApiKey))
        {
            throw LlmProviderErrors.NotConfigured(LlmProvider.Anthropic);
        }

        AnthropicClient client = new() { ApiKey = config.ApiKey };

        try
        {
            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model = model,
                MaxTokens = 16000,
                System = systemPrompt,
                Messages = history.Select(m => new MessageParam
                {
                    Role = m.Role == "assistant" ? Role.Assistant : Role.User,
                    Content = m.Content,
                }).ToList(),
            }, ct);

            return string.Join("\n", response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        }
        catch (AnthropicApiException ex)
        {
            throw LlmProviderErrors.RequestFailed(LlmProvider.Anthropic, ex.Message);
        }
    }
}
