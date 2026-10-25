import { useEffect, useRef, useState } from "react";
import type { StatusLevel } from "../../../lib/status";
import { ChevronDownIcon } from "./icons";
import selectStyles from "./Select.module.scss";
import styles from "./StatusSelect.module.scss";

export interface StatusSelectOption<T extends string> {
  value: T;
  label: string;
  level: StatusLevel;
}

interface StatusSelectProps<T extends string> {
  value: T;
  onChange: (value: T) => void;
  options: StatusSelectOption<T>[];
  ariaLabel: string;
}

export default function StatusSelect<T extends string>({ value, onChange, options, ariaLabel }: StatusSelectProps<T>) {
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
    <div className={styles.statusSelect} ref={rootRef}>
      <button
        type="button"
        className={`${selectStyles.selectTrigger} ${styles.statusTrigger} ${styles[current?.level ?? "muted"]}`}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={ariaLabel}
        onClick={() => setOpen((o) => !o)}
      >
        <span className={`${selectStyles.selectValue} ${styles.statusValue}`}>
          <span className={`led ${current?.level ?? "muted"}`} />
          {current?.label ?? ""}
        </span>
        <ChevronDownIcon className={`${selectStyles.chevron}${open ? ` ${styles.chevronOpen}` : ""}`} />
      </button>

      {open && (
        <ul
          className={`${selectStyles.selectDropdown} ${styles.statusSelectDropdown}`}
          role="listbox"
          aria-label={ariaLabel}
        >
          {options.map((opt) => (
            <li key={opt.value}>
              <button
                type="button"
                className={`${selectStyles.selectOption} ${styles.statusOption}`}
                role="option"
                aria-selected={opt.value === value}
                onClick={() => {
                  onChange(opt.value);
                  setOpen(false);
                }}
              >
                <span className={`led ${opt.level}`} />
                {opt.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
