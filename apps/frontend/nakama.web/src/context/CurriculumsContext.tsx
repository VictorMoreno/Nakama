import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { mockCurriculums } from "../data/mockCurriculums";
import { slugify } from "../lib/text";
import type { Curriculum, CurriculumLifecycleStatus, CurriculumMap, CurriculumModule } from "../types/curriculum";

export interface CreateCurriculumInput {
  title: string;
  audience: string;
  description: string;
  status: CurriculumLifecycleStatus;
  linkedChatbotKey?: string;
  iconUrl?: string;
}

interface CurriculumsContextValue {
  curriculums: CurriculumMap;
  activeCurriculums: Curriculum[];

  createCurriculum: (input: CreateCurriculumInput) => string;
  archiveCurriculum: (key: string) => void;
  updateStatus: (key: string, status: CurriculumLifecycleStatus) => void;
  updateIcon: (key: string, iconUrl: string | undefined) => void;
  addModule: (key: string, module: Omit<CurriculumModule, "id">) => void;
  removeModule: (key: string, moduleId: string) => void;
  toggleModuleGating: (key: string, moduleId: string) => void;
  completeModule: (key: string, moduleId: string, score?: number) => void;
}

const CurriculumsContext = createContext<CurriculumsContextValue | null>(null);

export function CurriculumsProvider({ children }: { children: ReactNode }) {
  const [curriculums, setCurriculums] = useState<CurriculumMap>(mockCurriculums);

  const activeCurriculums = useMemo(
    () => Object.values(curriculums).filter((c) => !c.archived),
    [curriculums]
  );

  function updateCurriculum(key: string, updater: (c: Curriculum) => Curriculum) {
    setCurriculums((prev) => {
      const c = prev[key];
      if (!c) return prev;
      return { ...prev, [key]: updater(c) };
    });
  }

  function createCurriculum(input: CreateCurriculumInput): string {
    const { title, audience, description, status, linkedChatbotKey, iconUrl } = input;
    const key = slugify(title, Object.keys(curriculums));
    const trimmedDescription = description.trim();
    const newCurriculum: Curriculum = {
      key,
      title,
      audience: audience || "Sin definir",
      description: trimmedDescription || "Currículo recién creado. Añade módulos para que el equipo pueda empezar a formarse.",
      status,
      linkedChatbotKey,
      iconUrl,
      modules: [],
      myProgress: { completedModuleIds: [], examScores: {} },
      team: []
    };
    setCurriculums((prev) => ({ ...prev, [key]: newCurriculum }));
    return key;
  }

  function archiveCurriculum(key: string) {
    updateCurriculum(key, (c) => ({ ...c, archived: true }));
  }

  function updateStatus(key: string, status: CurriculumLifecycleStatus) {
    updateCurriculum(key, (c) => ({ ...c, status }));
  }

  function updateIcon(key: string, iconUrl: string | undefined) {
    updateCurriculum(key, (c) => ({ ...c, iconUrl }));
  }

  function addModule(key: string, module: Omit<CurriculumModule, "id">) {
    updateCurriculum(key, (c) => {
      const id = slugify(module.title, c.modules.map((m) => m.id));
      return { ...c, modules: [...c.modules, { ...module, id }] };
    });
  }

  function removeModule(key: string, moduleId: string) {
    updateCurriculum(key, (c) => ({
      ...c,
      modules: c.modules.filter((m) => m.id !== moduleId),
      myProgress: {
        ...c.myProgress,
        completedModuleIds: c.myProgress.completedModuleIds.filter((id) => id !== moduleId)
      }
    }));
  }

  function toggleModuleGating(key: string, moduleId: string) {
    updateCurriculum(key, (c) => ({
      ...c,
      modules: c.modules.map((m) => (m.id === moduleId ? { ...m, gated: !m.gated } : m))
    }));
  }

  function completeModule(key: string, moduleId: string, score?: number) {
    updateCurriculum(key, (c) => {
      if (c.myProgress.completedModuleIds.includes(moduleId)) return c;
      return {
        ...c,
        myProgress: {
          completedModuleIds: [...c.myProgress.completedModuleIds, moduleId],
          examScores: score != null ? { ...c.myProgress.examScores, [moduleId]: score } : c.myProgress.examScores
        }
      };
    });
  }

  const value: CurriculumsContextValue = {
    curriculums,
    activeCurriculums,
    createCurriculum,
    archiveCurriculum,
    updateStatus,
    updateIcon,
    addModule,
    removeModule,
    toggleModuleGating,
    completeModule
  };

  return <CurriculumsContext.Provider value={value}>{children}</CurriculumsContext.Provider>;
}

export function useCurriculums(): CurriculumsContextValue {
  const ctx = useContext(CurriculumsContext);
  if (!ctx) throw new Error("useCurriculums must be used within a CurriculumsProvider");
  return ctx;
}
