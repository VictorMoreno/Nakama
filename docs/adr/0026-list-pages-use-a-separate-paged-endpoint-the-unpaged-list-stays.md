# ADR-26: list pages use a separate `GET /{resource}/paged` endpoint; the unpaged list stays

- **Status:** Accepted
- **Area:** Application layer — queries / API

The chatbot and formation list pages are paginated on the server, with the search text and the status filter applied
there too: a client-side filter over one page would only search the rows already downloaded. Each resource gets a
second endpoint, `GET /chatbots/paged` and `GET /formations/paged` (`page`, `pageSize`, `search`, `status`), backed by its
own `PageSearcher` use case and `...PageSpecification`, and answering a `PagedResponse<T>(Items, TotalCount, Page,
PageSize)`. The existing `GET /chatbots` and `GET /formations` are untouched.

They are kept on purpose. The frontend contexts load the whole list once and several screens depend on that map — the
global bot switcher, the chat, analytics and the detail pages — so paginating `GET /` would silently truncate all of them.
Moving those consumers to their own queries is a separate refactor; until then the unpaged endpoint is a deliberate
second read of the same read model, not an oversight.

The page size is chosen by the client (`pageSize`), but the server owns the limits: `Pagination:DefaultPageSize` is used
when the client sends none and `Pagination:MaxPageSize` is a hard cap, because an uncapped `pageSize` would defeat the
pagination. A `page` below 1 or a `pageSize` outside `1..MaxPageSize` is a `DomainError` (422), not a silent clamp. The
frontend mirrors its own page-size options in `VITE_*` and must not offer more than the backend cap.

The repository returns the page and its total together: `NhRepositoryBase.SearchPage(specification)` answers a
`PagedResult<T>(Items, TotalCount)` from one specification whose `Skip`/`Take` select the page and whose criteria also
feed the count, so the use case builds a single specification and the two can never filter differently. Both queries are
issued as NHibernate futures in one session; whether the Npgsql driver batches them into a single round trip was not
verified. The domain result carries only what the database knows — the page number, the page size and the page count
belong to the request, so the use case completes them: `PagedResponse<T>` adds `Page`, `PageSize` and a computed
`TotalPages` from its `PageRequest`. Archived rows are always excluded; no list page shows them. Search is a
case-insensitive substring match (`ToLower().Contains`), so a `%` or `_` typed by the user is not escaped and behaves as a
wildcard.

**Where it applies:** `ChatbotSummaryPageSearcher`, `FormationSummaryPageSearcher`, `ChatbotSummaryPageSpecification`,
`FormationSummaryPageSpecification`, `Shared/Application/PageRequest.cs`, `Shared/Infrastructure/Pagination/PaginationOptions.cs`.
