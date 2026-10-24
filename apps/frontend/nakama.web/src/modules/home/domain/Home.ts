export interface HomeFormation {
  formationId: string;
  title: string;
  progressPct: number;
  completedModules: number;
  modulesCount: number;
  nextModuleTitle?: string;
  completed: boolean;
}

export interface HomePendingExam {
  formationId: string;
  formationTitle: string;
  moduleId: string;
  moduleTitle: string;
  lastScore: number;
  passingScore: number;
}

export interface HomeSummary {
  questionsThisWeek: number;
  activeDaysLast30: number;
  formations: HomeFormation[];
  pendingExams: HomePendingExam[];
}
