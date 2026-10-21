import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import type { Page, PageQuery } from "../../../shared/domain/Page";
import { paginationConfig } from "../../../shared/infrastructure/config/PaginationConfig";

const SEARCH_DEBOUNCE_MS = 300;

type StatusFilter<TStatus extends string> = "all" | TStatus;

export type PageSlide = "next" | "prev" | "none";

interface LoadedPage<T> {
  key: string;
  filters: string;
  page: number;
  slide: PageSlide;
  data: Page<T>;
}

function parsePositiveInt(raw: string | null): number | null {
  const value = Number(raw);
  return raw !== null && Number.isInteger(value) && value > 0 ? value : null;
}

/**
 * Server-side paginated list whose page, page size, search text and status filter live in the URL
 * (`?page=&size=&q=&status=`), so reloading or going back restores the same view. The previous page
 * stays on screen while the next one loads.
 */
export function usePagedList<T, TStatus extends string>(
  fetchPage: (query: PageQuery<TStatus>) => Promise<Page<T>>,
  statuses: readonly TStatus[]
) {
  const [params, setParams] = useSearchParams();
  const [searchDraft, setSearchDraft] = useState<string | null>(null);
  const [loaded, setLoaded] = useState<LoadedPage<T> | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  const page = parsePositiveInt(params.get("page")) ?? 1;
  const requestedSize = parsePositiveInt(params.get("size"));
  const pageSize =
    requestedSize !== null && paginationConfig.pageSizeOptions.includes(requestedSize) ? requestedSize : paginationConfig.defaultPageSize;
  const search = params.get("q") ?? "";
  const requestedStatus = params.get("status");
  const status: StatusFilter<TStatus> = statuses.find((candidate) => candidate === requestedStatus) ?? "all";

  const filters = `${pageSize}|${search}|${status}`;
  const key = `${page}|${filters}|${reloadToken}`;
  const isLoading = loaded?.key !== key;
  const data = loaded?.data;
  const totalCount = data?.totalCount ?? 0;
  const totalPages = Math.max(1, data?.totalPages ?? 1);

  function updateParams(changes: Record<string, string | null>) {
    setParams((previous) => {
      const next = new URLSearchParams(previous);
      for (const [name, value] of Object.entries(changes)) {
        if (value === null || value === "") next.delete(name);
        else next.set(name, value);
      }
      return next;
    });
  }

  useEffect(() => {
    let cancelled = false;
    fetchPage({ page, pageSize, search, status: status === "all" ? undefined : status }).then((result) => {
      if (cancelled) return;
      if (result.items.length === 0 && result.totalCount > 0 && page > 1) {
        const lastPage = result.totalPages;
        setParams(
          (previous) => {
            const next = new URLSearchParams(previous);
            if (lastPage <= 1) next.delete("page");
            else next.set("page", String(lastPage));
            return next;
          },
          { replace: true }
        );
        return;
      }
      setLoaded((previous) => ({
        key,
        filters,
        page,
        slide: previous && previous.filters === filters && previous.page !== page ? (page > previous.page ? "next" : "prev") : "none",
        data: result
      }));
    });
    return () => {
      cancelled = true;
    };
  }, [fetchPage, page, pageSize, search, status, key, filters, setParams]);

  useEffect(() => {
    if (searchDraft === null) return;
    const timer = setTimeout(() => {
      const trimmed = searchDraft.trim();
      setSearchDraft(null);
      if (trimmed === search) return;
      setParams(
        (previous) => {
          const next = new URLSearchParams(previous);
          if (trimmed === "") next.delete("q");
          else next.set("q", trimmed);
          next.delete("page");
          return next;
        },
        { replace: true }
      );
    }, SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(timer);
  }, [searchDraft, search, setParams]);

  return {
    items: data?.items ?? [],
    slide: loaded?.slide ?? "none",
    displayedPage: loaded?.page ?? page,
    totalCount,
    page,
    pageSize,
    totalPages,
    isLoading,
    search: searchDraft ?? search,
    setSearch: setSearchDraft,
    status,
    setStatus: (value: StatusFilter<TStatus>) => updateParams({ status: value === "all" ? null : value, page: null }),
    setPage: (value: number) => updateParams({ page: value <= 1 ? null : String(value) }),
    setPageSize: (value: number) =>
      updateParams({ size: value === paginationConfig.defaultPageSize ? null : String(value), page: null }),
    reload: () => setReloadToken((token) => token + 1)
  };
}
