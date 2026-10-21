using Nakama.Shared.Domain;

namespace Nakama.Shared.Application;

public sealed record PageRequest(int Page, int PageSize)
{
    public int Skip => (this.Page - 1) * this.PageSize;

    public static PageRequest From(int page, int? pageSize, int defaultPageSize, int maxPageSize)
    {
        int size = pageSize ?? defaultPageSize;

        if (page < 1)
        {
            throw PaginationErrors.InvalidPage;
        }

        if (size < 1 || size > maxPageSize)
        {
            throw PaginationErrors.InvalidPageSize(maxPageSize);
        }

        return new PageRequest(page, size);
    }
}
