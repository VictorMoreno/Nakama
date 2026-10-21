import { readPositiveInt, readPositiveIntList } from "./EnvReaders";

const pageSizeOptions = readPositiveIntList("VITE_PAGINATION_PAGE_SIZE_OPTIONS", import.meta.env.VITE_PAGINATION_PAGE_SIZE_OPTIONS);
const defaultPageSize = readPositiveInt("VITE_PAGINATION_DEFAULT_PAGE_SIZE", import.meta.env.VITE_PAGINATION_DEFAULT_PAGE_SIZE);

if (!pageSizeOptions.includes(defaultPageSize)) {
  throw new Error("Invalid environment variable VITE_PAGINATION_DEFAULT_PAGE_SIZE: it must be one of VITE_PAGINATION_PAGE_SIZE_OPTIONS.");
}

export const paginationConfig = { defaultPageSize, pageSizeOptions } as const;
