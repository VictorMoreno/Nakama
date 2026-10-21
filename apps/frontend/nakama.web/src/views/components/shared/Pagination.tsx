import Select from "./Select";
import { ChevronLeftIcon } from "./icons";
import styles from "./Pagination.module.scss";

interface Props {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  pageSizeOptions: readonly number[];
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

const ELLIPSIS = "…";

function visiblePages(page: number, totalPages: number): (number | typeof ELLIPSIS)[] {
  const wanted = new Set([1, totalPages, page - 1, page, page + 1].filter((n) => n >= 1 && n <= totalPages));
  const sorted = [...wanted].sort((a, b) => a - b);
  const result: (number | typeof ELLIPSIS)[] = [];
  sorted.forEach((n, index) => {
    if (index > 0 && n - sorted[index - 1] > 1) result.push(ELLIPSIS);
    result.push(n);
  });
  return result;
}

export default function Pagination({ page, pageSize, totalCount, totalPages, pageSizeOptions, onPageChange, onPageSizeChange }: Props) {
  if (totalCount === 0) return null;

  const first = (page - 1) * pageSize + 1;
  const last = Math.min(page * pageSize, totalCount);

  return (
    <nav className={styles.pagination} aria-label="Paginación">
      <span className={styles.summary}>
        {first}–{last} de {totalCount}
      </span>

      <div className={styles.pages}>
        <button
          className={styles.pageButton}
          type="button"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          aria-label="Página anterior"
        >
          <ChevronLeftIcon />
        </button>
        {visiblePages(page, totalPages).map((item, index) =>
          item === ELLIPSIS ? (
            <span className={styles.ellipsis} key={`ellipsis-${index}`}>
              {ELLIPSIS}
            </span>
          ) : (
            <button
              className={`${styles.pageButton}${item === page ? ` ${styles.current}` : ""}`}
              type="button"
              key={item}
              onClick={() => onPageChange(item)}
              aria-label={`Página ${item}`}
              aria-current={item === page ? "page" : undefined}
            >
              {item}
            </button>
          )
        )}
        <button
          className={`${styles.pageButton} ${styles.next}`}
          type="button"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          aria-label="Página siguiente"
        >
          <ChevronLeftIcon />
        </button>
      </div>

      <div className={styles.size}>
        <span>Por página</span>
        <Select
          value={String(pageSize)}
          onChange={(value) => onPageSizeChange(Number(value))}
          options={pageSizeOptions.map((option) => ({ value: String(option), label: String(option) }))}
          ariaLabel="Elementos por página"
          openUp
        />
      </div>
    </nav>
  );
}
