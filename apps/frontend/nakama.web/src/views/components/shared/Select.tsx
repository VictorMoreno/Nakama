import { useEffect, useRef, useState } from "react";
import { ChevronDownIcon } from "./icons";
import styles from "./Select.module.scss";

interface SelectOption<T extends string> {
  value: T;
  label: string;
}

interface SelectProps<T extends string> {
  id?: string;
  value: T;
  onChange: (value: T) => void;
  options: SelectOption<T>[];
  ariaLabel: string;
  /** Stretch to fill the parent, matching the old `style={{ width: "100%" }}` native selects used in form fields. */
  fullWidth?: boolean;
  /** Open the panel above the trigger, for selects pinned to the bottom edge where opening downwards would force a scroll. */
  openUp?: boolean;
}

export default function Select<T extends string>({ id, value, onChange, options, ariaLabel, fullWidth, openUp }: SelectProps<T>) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const current = options.find((o) => o.value === value);

  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  return (
    <div
      className={`${styles.select}${open ? ` ${styles.open}` : ""}${fullWidth ? ` ${styles.selectFull}` : ""}${openUp ? ` ${styles.openUp}` : ""}`}
      ref={rootRef}
    >
      <button
        id={id}
        className={styles.selectTrigger}
        type="button"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={ariaLabel}
        onClick={() => setOpen((o) => !o)}
      >
        <span className={styles.selectValueBox}>
          <span className={styles.selectValue}>{current?.label ?? ""}</span>
          {options.map((opt) => (
            <span key={opt.value} className={styles.selectSizer} aria-hidden="true">
              {opt.label}
            </span>
          ))}
        </span>
        <ChevronDownIcon className={styles.chevron} />
      </button>

      {open && (
        <ul className={styles.selectDropdown} role="listbox" aria-label={ariaLabel}>
          {options.map((opt) => (
            <li key={opt.value}>
              <button
                type="button"
                className={styles.selectOption}
                role="option"
                aria-selected={opt.value === value}
                onClick={() => {
                  onChange(opt.value);
                  setOpen(false);
                }}
              >
                {opt.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
