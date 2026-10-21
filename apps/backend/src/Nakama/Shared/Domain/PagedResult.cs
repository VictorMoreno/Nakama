namespace Nakama.Shared.Domain;

public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount);
