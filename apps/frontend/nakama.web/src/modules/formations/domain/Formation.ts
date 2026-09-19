export type ModuleType = "video" | "documento" | "ejercicio" | "examen";

/** Estado configurado por el admin del currículo — determina el LED de la card y el status-tag de la ficha */
export type FormationLifecycleStatus = "activo" | "inactivo" | "borrador";

export interface QuizQuestion {
  question: string;
  options: string[];
  correctIndex: number;
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
  /** Contenido generado como borrador por IA a partir de los documentos indexados del chatbot de referencia, pendiente de revisión */
  aiDraft: boolean;
  /** Enlace al recurso — vídeo, documento o ejercicio, según el tipo */
  resourceUrl?: string;
  /** Solo para type "video" */
  durationMin?: number;
  /** Solo para type "examen" */
  quiz?: QuizQuestion[];
  /** Solo para type "examen": nota mínima (0-100) para aprobar */
  passingScore?: number;
}

/** Progreso del usuario actual en un currículo */
export interface LearnerProgress {
  completedModuleIds: string[];
  examScores: Record<string, number>;
}

export interface FormationMember {
  name: string;
  progress: number;
}

/**
 * Un currículo de formación (RF23) — entidad independiente del chatbot. Puede referenciar
 * opcionalmente un chatbot (solo como fuente de contexto para el borrador de IA y para
 * mostrar de dónde sale el contenido), pero no depende de él para nada más.
 */
export interface Formation {
  id: string;
  title: string;
  /** Etiqueta libre: a quién va dirigido, p. ej. "Diagnósticos ECU" o "Toda la empresa" */
  audience: string;
  description: string;
  /** Configurado por el admin del currículo, independiente del progreso de aprendizaje */
  status: FormationLifecycleStatus;
  /** Chatbot key opcional, solo para contexto de generación por IA */
  linkedChatbotKey?: string;
  modules: FormationModule[];
  myProgress: LearnerProgress;
  team: FormationMember[];
  archived?: boolean;
  /** Imagen subida por un admin (al crear o desde la ficha del currículo), mostrada en la card del listado en vez del icono por defecto */
  iconUrl?: string;
}

export type FormationMap = Record<string, Formation>;
