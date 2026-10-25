export type ModuleType = "video" | "documento" | "ejercicio" | "examen";

export type FormationLifecycleStatus = "activo" | "inactivo" | "borrador";

export const MAX_GENERATED_QUESTIONS = 4;

export const MIN_PASSING_SCORE = 1;
export const MAX_PASSING_SCORE = 100;

export interface QuizQuestion {
  question: string;
  options: string[];
  correctIndex?: number;
}

export interface FormationModule {
  id: string;
  title: string;
  type: ModuleType;
  summary: string;
  gated: boolean;
  aiDraft: boolean;
  resourceUrl?: string;
  durationMin?: number;
  quiz?: QuizQuestion[];
  passingScore?: number;
}

export interface LearnerProgress {
  completedModuleIds: string[];
  examScores: Record<string, number>;
  examAttempts: ExamAttempt[];
}

export interface ExamAttempt {
  moduleId: string;
  score: number;
  passed: boolean;
  attemptedAt: string;
}

export interface ExamResult {
  score: number;
  passed: boolean;
}

export interface FormationDocument {
  id: string;
  name: string;
  origin: string;
  updated: string;
}

export interface FormationMember {
  userId: string;
  progress: number;
}

export interface Formation {
  id: string;
  title: string;
  audience: string;
  description: string;
  status: FormationLifecycleStatus;
  modules: FormationModule[];
  docs: FormationDocument[];
  team: FormationMember[];
  archived?: boolean;
  iconUrl?: string;
  isEnrolled: boolean;
  progressPct: number;
  completedModulesCount: number;
}

export type FormationMap = Record<string, Formation>;

export interface FormationListItem {
  id: string;
  title: string;
  audience: string;
  description: string;
  status: FormationLifecycleStatus;
  iconUrl?: string;
  isEnrolled: boolean;
  progressPct: number;
}
