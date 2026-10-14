using System.Runtime.CompilerServices;
using Nakama.Shared.Domain;
using Test.Modules.Chatbots;

namespace Test.Api.Support;

public sealed class FakeLlmClient : ILlmClient
{
    private readonly List<LlmRequest> _requests = [];

    private string[] _chunks = ["Hello", " world"];

    private Exception? _failure;

    public IReadOnlyList<LlmRequest> Requests => this._requests;

    public void Answer(params string[] chunks)
    {
        this._chunks = chunks;
        this._failure = null;
    }

    public void FailAfter(string[] chunks, Exception failure)
    {
        this._chunks = chunks;
        this._failure = failure;
    }

    public async IAsyncEnumerable<string> AskStream(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> history,
        IReadOnlyList<LlmDocument> documents,
        [EnumeratorCancellation] CancellationToken ct)
    {
        this._requests.Add(new LlmRequest(model, systemPrompt, history, documents));

        foreach (string chunk in this._chunks)
        {
            await Task.Yield();
            yield return chunk;
        }

        if (this._failure is not null)
        {
            throw this._failure;
        }
    }
}
