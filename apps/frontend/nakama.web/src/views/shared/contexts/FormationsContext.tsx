/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import type { ExamResult, Formation, FormationMap, FormationModule } from "../../../modules/formations/domain/Formation";
import type { CreateFormationInput, IFormationsRepository } from "../../../modules/formations/domain/IFormationsRepository";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";
import { useUser } from "./UserContext";

export type { CreateFormationInput };

interface FormationsContextValue {
  formations: FormationMap;
  activeFormations: Formation[];
  isLoading: boolean;

  createFormation: (input: CreateFormationInput) => Promise<string>;
  archiveFormation: (key: string) => Promise<void>;
  unarchiveFormation: (key: string) => Promise<void>;
  updateStatus: (key: string, status: Formation["status"]) => Promise<void>;
  updateDetails: (key: string, title: string, audience: string, description: string) => Promise<void>;
  updateIcon: (key: string, iconUrl: string | undefined) => Promise<void>;
  addModule: (key: string, module: Omit<FormationModule, "id">) => Promise<void>;
  updateModule: (key: string, module: FormationModule) => Promise<void>;
  removeModule: (key: string, moduleId: string) => Promise<void>;
  addDocument: (key: string, file: File, name: string, origin: string) => Promise<void>;
  removeDocument: (key: string, documentId: string) => Promise<void>;
  toggleModuleGating: (key: string, moduleId: string) => Promise<void>;
  completeModule: (key: string, moduleId: string) => Promise<void>;
  submitExam: (key: string, moduleId: string, answers: number[]) => Promise<ExamResult>;
  addTeamMember: (key: string, userId: string) => Promise<void>;
  removeTeamMember: (key: string, userId: string) => Promise<void>;
}

const FormationsContext = createContext<FormationsContextValue | null>(null);

export function FormationsProvider({ children }: { children: ReactNode }) {
  const repo = useInjection<IFormationsRepository>(Services.FormationsRepository.id);
  const { currentUser } = useUser();

  const [formations, setFormations] = useState<FormationMap>({});
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!currentUser) {
      setFormations({});
      setIsLoading(false);
      return;
    }
    let cancelled = false;
    setIsLoading(true);
    repo.FindAll().then((data) => {
      if (!cancelled) {
        setFormations(data);
        setIsLoading(false);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [repo, currentUser]);

  const activeFormations = useMemo(
    () => Object.values(formations).filter((c) => !c.archived).sort((a, b) => a.title.localeCompare(b.title)),
    [formations]
  );

  async function refresh() {
    if (!currentUser) return;
    setFormations(await repo.FindAll());
  }

  async function createFormation(input: CreateFormationInput): Promise<string> {
    const c = await repo.Create(input);
    setFormations((prev) => ({ ...prev, [c.id]: c }));
    return c.id;
  }

  async function archiveFormation(key: string) {
    await repo.Archive(key);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], archived: true } } : prev));
  }

  async function unarchiveFormation(key: string) {
    await repo.Unarchive(key);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], archived: false } } : prev));
  }

  async function updateStatus(key: string, status: Formation["status"]) {
    await repo.UpdateStatus(key, status);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], status } } : prev));
  }

  async function updateDetails(key: string, title: string, audience: string, description: string) {
    await repo.UpdateDetails(key, title, audience, description);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], title, audience, description } } : prev));
  }

  async function updateIcon(key: string, iconUrl: string | undefined) {
    await repo.UpdateIcon(key, iconUrl);
    setFormations((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], iconUrl } } : prev));
  }

  async function addModule(key: string, module: Omit<FormationModule, "id">) {
    await repo.AddModule(key, module);
    await refresh();
  }

  async function updateModule(key: string, module: FormationModule) {
    await repo.UpdateModule(key, module);
    await refresh();
  }

  async function removeModule(key: string, moduleId: string) {
    await repo.RemoveModule(key, moduleId);
    await refresh();
  }

  async function addDocument(key: string, file: File, name: string, origin: string) {
    await repo.AddDocument(key, file, name, origin);
    await refresh();
  }

  async function removeDocument(key: string, documentId: string) {
    await repo.RemoveDocument(key, documentId);
    await refresh();
  }

  async function toggleModuleGating(key: string, moduleId: string) {
    await repo.ToggleModuleGating(key, moduleId);
    await refresh();
  }

  async function completeModule(key: string, moduleId: string) {
    if (!currentUser) return;
    await repo.CompleteModule(key, moduleId);
    await refresh();
  }

  async function submitExam(key: string, moduleId: string, answers: number[]): Promise<ExamResult> {
    const result = await repo.SubmitExam(key, moduleId, answers);
    await refresh();
    return result;
  }

  async function addTeamMember(key: string, userId: string) {
    await repo.AddTeamMember(key, userId);
    await refresh();
  }

  async function removeTeamMember(key: string, userId: string) {
    await repo.RemoveTeamMember(key, userId);
    await refresh();
  }

  const value: FormationsContextValue = {
    formations,
    activeFormations,
    isLoading,
    createFormation,
    archiveFormation,
    unarchiveFormation,
    updateStatus,
    updateDetails,
    updateIcon,
    addModule,
    updateModule,
    removeModule,
    addDocument,
    removeDocument,
    toggleModuleGating,
    completeModule,
    submitExam,
    addTeamMember,
    removeTeamMember
  };

  return <FormationsContext.Provider value={value}>{children}</FormationsContext.Provider>;
}

export function useFormations(): FormationsContextValue {
  const ctx = useContext(FormationsContext);
  if (!ctx) throw new Error("useFormations must be used within a FormationsProvider");
  return ctx;
}
