using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Nakama.LlmProviders.Domain;
using Nakama.LlmProviders.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class AnthropicLlmClient(ILlmProviderConfigRepository repository) : ILlmClient
{
    private const int DefaultMaxTokens = 16000;

    public async IAsyncEnumerable<string> AskStream(string model, string systemPrompt, IReadOnlyList<LlmMessage> history,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var config = await repository.FindByProvider(LlmProvider.Anthropic, ct);
        if (config is null || !config.Enabled || string.IsNullOrEmpty(config.ApiKey))
        {
            throw LlmProviderErrors.NotConfigured(LlmProvider.Anthropic);
        }

        AnthropicClient client = new() { ApiKey = config.ApiKey };

        var parameters = new MessageCreateParams
        {
            Model = model,
            MaxTokens = DefaultMaxTokens,
            System = systemPrompt,
            Messages = history.Select(m => new MessageParam
            {
                Role = m.Role == "assistant" ? Role.Assistant : Role.User,
                Content = m.Content,
            }).ToList()
        };

        await using IAsyncEnumerator<RawMessageStreamEvent> events = client.Messages.CreateStreaming(parameters, cancellationToken: ct)
            .GetAsyncEnumerator(ct);
        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = await events.MoveNextAsync();
            }
            catch (AnthropicApiException ex)
            {
                throw LlmProviderErrors.RequestFailed(LlmProvider.Anthropic, ex.Message);
            }

            if (!hasNext)
            {
                yield break;
            }

            if (events.Current.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
            {
                yield return text.Text;
            }
        }
    }
}
