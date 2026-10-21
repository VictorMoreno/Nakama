namespace Nakama.Shared.Infrastructure.Pagination;

public sealed class PaginationOptions
{
    public const string SectionName = "Pagination";

    public required int DefaultPageSize { get; init; }

    public required int MaxPageSize { get; init; }
}
