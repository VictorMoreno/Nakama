namespace Nakama.Shared.Domain;

public sealed class PaginationInvalidPageError()
    : DomainError("nakama.pagination.invalid_page", "The page number must be greater than zero.");

public sealed class PaginationInvalidPageSizeError(int maxPageSize)
    : DomainError("nakama.pagination.invalid_page_size", $"The page size must be between 1 and {maxPageSize}.");

public static class PaginationErrors
{
    public static DomainError InvalidPage { get; } = new PaginationInvalidPageError();

    public static DomainError InvalidPageSize(int maxPageSize) => new PaginationInvalidPageSizeError(maxPageSize);
}
