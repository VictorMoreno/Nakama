using System.Runtime.CompilerServices;
using System.Text;
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

    public async IAsyncEnumerable<string> AskStream(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> history,
        IReadOnlyList<LlmDocument> documents,
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
            Messages = BuildMessages(history, documents),
            CacheControl = new CacheControlEphemeral()
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

    private static List<MessageParam> BuildMessages(IReadOnlyList<LlmMessage> history, IReadOnlyList<LlmDocument> documents)
    {
        var messages = new List<MessageParam>(history.Count);
        for (var i = 0; i < history.Count; i++)
        {
            var message = history[i];
            var role = message.Role == "assistant" ? Role.Assistant : Role.User;

            if (i == 0 && documents.Count > 0)
            {
                var blocks = documents.Select(ToDocumentBlock).ToList();
                blocks.Add(new TextBlockParam { Text = message.Content });
                messages.Add(new MessageParam { Role = role, Content = blocks });
                continue;
            }

            messages.Add(new MessageParam { Role = role, Content = message.Content });
        }

        return messages;
    }

    private static ContentBlockParam ToDocumentBlock(LlmDocument document) =>
        new DocumentBlockParam
        {
            Title = document.Name,
            Source = document.Kind == LlmDocumentKind.Pdf
                ? new Base64PdfSource { Data = Convert.ToBase64String(document.Content) }
                : new PlainTextSource { Data = Encoding.UTF8.GetString(document.Content) }
        };
}
