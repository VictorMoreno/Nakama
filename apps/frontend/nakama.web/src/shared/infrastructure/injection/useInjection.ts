import { useMemo } from "react";
import { resolve } from "./Container";

export function useInjection<T>(id: string): T {
  return useMemo(() => resolve<T>(id), [id]);
}
