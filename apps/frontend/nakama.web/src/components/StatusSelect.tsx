import { useEffect, useRef, useState } from "react";
import type { StatusLevel } from "../lib/status";
import { ChevronDownIcon } from "./icons";

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

/**
 * The status-tag badge itself, made clickable — opens the same option list as `Select`
 * so admins configure the estado right where they already read it, instead of a separate
 * config row further down the page.
 */
export function StatusSelect<T extends string>({ value, onChange, options, ariaLabel }: StatusSelectProps<T>) {
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
    <div className={`status-select${open ? " open" : ""}`} ref={rootRef}>
      <button
        type="button"
        className={`select-trigger status-trigger ${current?.level ?? "muted"}`}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={ariaLabel}
        onClick={() => setOpen((o) => !o)}
      >
        <span className="select-value">
          <span className={`led ${current?.level ?? "muted"}`} />
          {current?.label ?? ""}
        </span>
        <ChevronDownIcon className="chevron" />
      </button>

      {open && (
        <ul className="select-dropdown status-select-dropdown" role="listbox" aria-label={ariaLabel}>
          {options.map((opt) => (
            <li key={opt.value}>
              <button
                type="button"
                className="select-option"
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
