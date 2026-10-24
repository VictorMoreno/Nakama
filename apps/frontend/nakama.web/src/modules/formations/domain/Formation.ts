export type ModuleType = "video" | "documento" | "ejercicio" | "examen";

/** Estado configurado por el admin del currículo — determina el LED de la card y el status-tag de la ficha */
export type FormationLifecycleStatus = "activo" | "inactivo" | "borrador";

export const MAX_GENERATED_QUESTIONS = 4;

export const MIN_PASSING_SCORE = 1;
export const MAX_PASSING_SCORE = 100;

export interface QuizQuestion {
  question: string;
  options: string[];
  /** Solo la reciben los administradores: a quien cursa la formación el servidor no le envía la respuesta correcta */
  correctIndex?: number;
}

/**
 * Un módulo de un currículo de formación (RF23). Los campos relevantes cambian según
 * `type`: video usa resourceUrl+durationMin, documento y ejercicio usan resourceUrl,
 * examen usa quiz+passingScore. Solo los administradores crean/editan módulos.
 */
export interface FormationModule {
  id: string;
  title: string;
  type: ModuleType;
  summary: string;
  /** Si está activo, el siguiente módulo queda bloqueado hasta completar este (configurable por el admin) */
  gated: boolean;
  /** Preguntas generadas por IA a partir de los documentos de la formación, pendientes de revisión: se marca al guardar tras generarlas y se limpia al editar y guardar el módulo */
  aiDraft: boolean;
  /** Enlace al recurso — vídeo, documento o ejercicio, según el tipo */
  resourceUrl?: string;
  /** Solo para type "video" */
  durationMin?: number;
  /** Solo para type "examen" */
  quiz?: QuizQuestion[];
  /** Solo para type "examen": nota mínima (1-100) para aprobar */
  passingScore?: number;
}

/** Progreso del usuario actual en un currículo */
export interface LearnerProgress {
  completedModuleIds: string[];
  examScores: Record<string, number>;
  /** Todos los intentos de examen de la persona, aprobados o no, en orden cronológico */
  examAttempts: ExamAttempt[];
}

export interface ExamAttempt {
  moduleId: string;
  score: number;
  passed: boolean;
  attemptedAt: string;
}

/** Resultado de entregar un examen, calculado por el servidor */
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

/**
 * Un currículo de formación (RF23) — entidad independiente del chatbot. Al redactar un examen
 * se pueden usar los documentos de un chatbot para generar un borrador de preguntas con IA,
 * pero la formación no guarda ninguna referencia a él.
 */
export interface Formation {
  id: string;
  title: string;
  /** Etiqueta libre: a quién va dirigido, p. ej. "Diagnósticos ECU" o "Toda la empresa" */
  audience: string;
  description: string;
  /** Configurado por el admin del currículo, independiente del progreso de aprendizaje */
  status: FormationLifecycleStatus;
  modules: FormationModule[];
  docs: FormationDocument[];
  team: FormationMember[];
  archived?: boolean;
  /** Imagen subida por un admin (al crear o desde la ficha del currículo), mostrada en la card del listado en vez del icono por defecto */
  iconUrl?: string;
  /** Si el usuario actual está asignado a esta formación (solo las personas asignadas pueden cursarla) */
  isEnrolled: boolean;
  /** Progreso del usuario actual (0-100), calculado en el backend */
  progressPct: number;
  completedModulesCount: number;
}

export type FormationMap = Record<string, Formation>;

/** Fila de las páginas de listado: solo el resumen, sin cargar el detalle de cada formación. */
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
