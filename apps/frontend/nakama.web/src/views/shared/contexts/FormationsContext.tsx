// Provider and hook are deliberately colocated (standard React Context convention) —
// this only costs Fast Refresh a full remount of consumers on edit, not correctness.
/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import type { Formation, FormationMap, FormationModule } from "../../../modules/formations/domain/Formation";
import type { CreateFormationInput, IFormationsRepository } from "../../../modules/formations/domain/IFormationsRepository";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";

export type { CreateFormationInput };

interface FormationsContextValue {
  formations: FormationMap;
  activeFormations: Formation[];
  isLoading: boolean;

  createFormation: (input: CreateFormationInput) => Promise<string>;
  archiveFormation: (key: string) => Promise<void>;
  updateStatus: (key: string, status: Formation["status"]) => Promise<void>;
  updateIcon: (key: string, iconUrl: string | undefined) => Promise<void>;
  addModule: (key: string, module: Omit<FormationModule, "id">) => Promise<void>;
  removeModule: (key: string, moduleId: string) => Promise<void>;
  toggleModuleGating: (key: string, moduleId: string) => Promise<void>;
  completeModule: (key: string, moduleId: string, score?: number) => Promise<void>;
}

const FormationsContext = createContext<FormationsContextValue | null>(null);

export function FormationsProvider({ children }: { children: ReactNode }) {
  const repo = useInjection<IFormationsRepository>(Services.FormationsRepository.id);

  const [formations, setFormations] = useState<FormationMap>({});
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    repo.FindAll().then((data) => {
      if (!cancelled) {
        setFormations(data);
        setIsLoading(false);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [repo]);

  const activeFormations = useMemo(() => Object.values(formations).filter((c) => !c.archived), [formations]);

  // Mutations that touch nested/derived state (modules, gating, progress) re-fetch from the
  // repository afterward instead of hand-rolling the same update logic twice — the
  // repository already owns the authoritative result of the mutation.
  async function refresh() {
    setFormations(await repo.FindAll());
  }

  async function createFormation(input: CreateFormationInput): Promise<string> {
    const c = await repo.Create(input);
    setFormations((prev) => ({ ...prev, [c.key]: c }));
    return c.key;
  }

  async function archiveFormation(key: string) {
    await repo.Archive(key);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], archived: true } } : prev));
  }

  async function updateStatus(key: string, status: Formation["status"]) {
    await repo.UpdateStatus(key, status);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], status } } : prev));
  }

  async function updateIcon(key: string, iconUrl: string | undefined) {
    await repo.UpdateIcon(key, iconUrl);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], iconUrl } } : prev));
  }

  async function addModule(key: string, module: Omit<FormationModule, "id">) {
    await repo.AddModule(key, module);
    await refresh();
  }

  async function removeModule(key: string, moduleId: string) {
    await repo.RemoveModule(key, moduleId);
    await refresh();
  }

  async function toggleModuleGating(key: string, moduleId: string) {
    await repo.ToggleModuleGating(key, moduleId);
    await refresh();
  }

  async function completeModule(key: string, moduleId: string, score?: number) {
    await repo.CompleteModule(key, moduleId, score);
    await refresh();
  }

  const value: FormationsContextValue = {
    formations,
    activeFormations,
    isLoading,
    createFormation,
    archiveFormation,
    updateStatus,
    updateIcon,
    addModule,
    removeModule,
    toggleModuleGating,
    completeModule
  };

  return <FormationsContext.Provider value={value}>{children}</FormationsContext.Provider>;
}

export function useFormations(): FormationsContextValue {
  const ctx = useContext(FormationsContext);
  if (!ctx) throw new Error("useFormations must be used within a FormationsProvider");
  return ctx;
}
