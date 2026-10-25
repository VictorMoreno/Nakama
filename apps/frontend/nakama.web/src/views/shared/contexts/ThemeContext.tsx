/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, type ReactNode } from "react";
import { useLocalStorageState } from "../hooks/useLocalStorageState";

export type ThemeMode = "light" | "dark";

interface ThemeContextValue {
  mode: ThemeMode;
  setMode: (mode: ThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);
const STORAGE_KEY = "nakama-theme";

function isThemeMode(value: unknown): value is ThemeMode {
  return value === "light" || value === "dark";
}

function systemDefault(): ThemeMode {
  if (typeof window !== "undefined" && window.matchMedia?.("(prefers-color-scheme: dark)").matches) return "dark";
  return "light";
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useLocalStorageState<ThemeMode>(STORAGE_KEY, systemDefault(), isThemeMode);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", mode);
  }, [mode]);

  return <ThemeContext.Provider value={{ mode, setMode }}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error("useTheme must be used within a ThemeProvider");
  return ctx;
}
