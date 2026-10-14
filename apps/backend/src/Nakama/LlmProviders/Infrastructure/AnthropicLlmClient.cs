using System.Runtime.CompilerServices;
using System.Text;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;
using Nakama.LlmProviders.Domain;
using Nakama.LlmProviders.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Llm;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class AnthropicLlmClient(ILlmProviderConfigRepository repository, IOptions<LlmOptions> options) : ILlmClient
{
    public async IAsyncEnumerable<string> AskStream(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> history,
        IReadOnlyList<LlmDocument> documents,
        [EnumeratorCancellation] CancellationToken ct)
    {
        IReadOnlyList<LlmProviderConfig> found = await repository.Search(
            new LlmProviderConfigByProviderSpecification(LlmProvider.Anthropic), ct);
        LlmProviderConfig? config = found.FirstOrDefault();
        if (config is null || !config.Enabled || string.IsNullOrEmpty(config.ApiKey))
        {
            throw LlmProviderErrors.NotConfigured(LlmProvider.Anthropic);
        }

        AnthropicClient client = new() { ApiKey = config.ApiKey };

        MessageCreateParams parameters = new()
        {
            Model = model,
            MaxTokens = options.Value.MaxTokens,
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

            if (events.Current.TryPickContentBlockDelta(out RawContentBlockDeltaEvent? delta) && delta.Delta.TryPickText(out TextDelta? text))
            {
                yield return text.Text;
            }
        }
    }

    private static List<MessageParam> BuildMessages(IReadOnlyList<LlmMessage> history, IReadOnlyList<LlmDocument> documents)
    {
        List<MessageParam> messages = new(history.Count);
        for (int i = 0; i < history.Count; i++)
        {
            LlmMessage message = history[i];
            Role role = message.Role == "assistant" ? Role.Assistant : Role.User;

            IReadOnlyList<LlmDocument> messageDocuments = message.Documents ?? [];
            List<LlmDocument> leadingDocuments = i == 0 ? documents.Concat(messageDocuments).ToList() : messageDocuments.ToList();

            if (leadingDocuments.Count > 0)
            {
                List<ContentBlockParam> blocks = leadingDocuments.Select(ToDocumentBlock).ToList();
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
