namespace Nakama.Shared.Application;

public sealed record PagedResponse<TItem>(IReadOnlyCollection<TItem> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)this.TotalCount / this.PageSize);

    public static PagedResponse<TItem> From(IReadOnlyCollection<TItem> items, int totalCount, PageRequest pageRequest) =>
        new(items, totalCount, pageRequest.Page, pageRequest.PageSize);
}
