using Nakama.Shared.Infrastructure.Pagination;

namespace Test.Modules.Shared;

public static class PaginationOptionsMother
{
    public static PaginationOptions Random() => With();

    public static PaginationOptions With(int? defaultPageSize = null, int? maxPageSize = null) =>
        new()
        {
            DefaultPageSize = defaultPageSize ?? 12,
            MaxPageSize = maxPageSize ?? 50
        };
}
