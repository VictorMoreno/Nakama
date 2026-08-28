import { useEffect, useState } from "react";

/**
 * Encapsulates reading/writing a piece of UI/session state to localStorage, so components
 * don't touch `localStorage` directly. `isValid` guards against corrupted/foreign values
 * already sitting under `key` (private-mode quirks, a stale shape from an older version, ...).
 */
export function useLocalStorageState<T>(key: string, initial: T, isValid: (value: unknown) => value is T): [T, (value: T) => void] {
  const [value, setValue] = useState<T>(() => {
    try {
      const raw = localStorage.getItem(key);
      if (raw == null) return initial;
      const parsed: unknown = JSON.parse(raw);
      return isValid(parsed) ? parsed : initial;
    } catch {
      return initial;
    }
  });

  useEffect(() => {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {
      // ignore persistence failures (private mode, storage quota, ...)
    }
  }, [key, value]);

  return [value, setValue];
}
