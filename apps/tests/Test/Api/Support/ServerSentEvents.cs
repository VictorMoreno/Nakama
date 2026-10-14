using System.Text.Json;

namespace Test.Api.Support;

public sealed record SseFrame(string Event, string Data);

public static class ServerSentEvents
{
    public static IReadOnlyList<SseFrame> Parse(string body) => body
        .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(block =>
        {
            string[] lines = block.Split('\n');

            return new SseFrame(
                lines.First(line => line.StartsWith("event: ", StringComparison.Ordinal))[7..],
                lines.First(line => line.StartsWith("data: ", StringComparison.Ordinal))[6..]);
        })
        .ToList();

    public static IReadOnlyList<string> Chunks(this IReadOnlyList<SseFrame> frames) => frames
        .Where(frame => frame.Event == "chunk")
        .Select(frame => JsonSerializer.Deserialize<string>(frame.Data)!)
        .ToList();
}
