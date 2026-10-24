export interface Page<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  page: number;
  pageSize: number;
}

export interface PageQuery<TStatus extends string> {
  page: number;
  pageSize: number;
  search: string;
  status?: TStatus;
  archived?: boolean;
}
