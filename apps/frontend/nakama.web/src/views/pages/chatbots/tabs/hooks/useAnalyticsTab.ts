import { useState } from "react";
import { useToast } from "../../../../shared/contexts/ToastContext";

export function useAnalyticsTab() {
  const { showToast } = useToast();
  const [documented, setDocumented] = useState<Set<number>>(new Set());

  function markDocumented(idx: number) {
    setDocumented((prev) => new Set(prev).add(idx));
    showToast("Añadida a la cola de documentación");
  }

  return { documented, markDocumented };
}
