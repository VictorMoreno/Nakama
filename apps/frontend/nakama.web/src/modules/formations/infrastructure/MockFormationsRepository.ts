import { slugify } from "../../../lib/text";
import type { Formation, FormationMap, FormationModule } from "../domain/Formation";
import type { CreateFormationInput, IFormationsRepository } from "../domain/IFormationsRepository";

/**
 * Placeholder data para RF23 (currículo de formación). Es una entidad independiente de
 * los chatbots — `linkedChatbotKey` es opcional y solo sirve de contexto para el borrador
 * generado por IA, nunca una dependencia funcional. Swapping this repository for
 * `ApiFormationsRepository` later is a data-layer change only.
 */
const seedFormations: FormationMap = {
  "onboarding-ecu": {
    id: "onboarding-ecu",
    title: "Onboarding Diagnósticos ECU",
    audience: "Diagnósticos ECU",
    description: "Formación guiada para incorporarse al equipo de Diagnósticos ECU: arquitectura de protocolos y aislamiento entre fabricantes.",
    status: "activo",
    linkedChatbotKey: "ecu",
    modules: [
      {
        id: "ecu-m1",
        title: "Arquitectura de protocolos ECU",
        type: "documento",
        summary: "Introducción al puerto IEcuProtocol y por qué EcuReader nunca depende de un fabricante concreto.",
        gated: false,
        aiDraft: false,
        resourceUrl: "https://docs.internal/ecu/arquitectura-protocolos-ecu"
      },
      {
        id: "ecu-m2",
        title: "Vídeo: añadir un protocolo nuevo",
        type: "video",
        summary: "Recorrido guiado por el caso real de incorporar un fabricante sin tocar el código existente.",
        gated: true,
        aiDraft: false,
        resourceUrl: "https://video.internal/ecu/nuevo-protocolo",
        durationMin: 8
      },
      {
        id: "ecu-m3",
        title: "Ejercicio: timeout configurable por fabricante",
        type: "ejercicio",
        summary: "Implementa un ConnectTimeoutMs distinto para un fabricante nuevo siguiendo el patrón de Volvo.",
        gated: true,
        aiDraft: true,
        resourceUrl: "https://tasks.internal/ecu/timeout-configurable"
      },
      {
        id: "ecu-m4",
        title: "Examen: protocolos y aislamiento entre fabricantes",
        type: "examen",
        summary: "Valida que entiendes por qué un fallo en un adaptador no puede romper la lectura de otro fabricante.",
        gated: true,
        aiDraft: false,
        passingScore: 70,
        quiz: [
          {
            question: "¿Por qué EcuReader no depende de clases concretas de protocolo?",
            options: ["Por rendimiento", "Para poder añadir fabricantes nuevos sin tocar el código existente", "Porque lo exige C#", "No hay ninguna razón especial"],
            correctIndex: 1
          },
          {
            question: "Si el adaptador de un fabricante falla, ¿qué ocurre con el resto?",
            options: ["Deja de funcionar toda la lectura", "Solo afecta a ese fabricante, el resto sigue funcionando", "Se reinicia la aplicación", "No es posible que falle"],
            correctIndex: 1
          }
        ]
      }
    ],
    myProgress: { completedModuleIds: ["ecu-m1"], examScores: {} },
    team: [
      { name: "Marta Vidal", progress: 100 },
      { name: "Jon Etxeberria", progress: 75 },
      { name: "Iker Sola", progress: 25 }
    ]
  },

  "onboarding-talleres": {
    id: "onboarding-talleres",
    title: "Onboarding Acceso Talleres",
    audience: "Acceso Talleres",
    description: "Formación guiada sobre el flujo de autenticación y gestión de sesiones para las tablets de taller.",
    status: "inactivo",
    linkedChatbotKey: "auth",
    modules: [
      {
        id: "auth-m1",
        title: "Flujo de autenticación con Jaltest Auth",
        type: "documento",
        summary: "PKCE, sesiones y por qué las tablets de taller son un cliente público.",
        gated: false,
        aiDraft: false,
        resourceUrl: "https://docs.internal/auth/seguridad-auth-talleres"
      },
      {
        id: "auth-m2",
        title: "Vídeo: revocar el acceso de una tablet perdida",
        type: "video",
        summary: "Caso real de revocación de sesión desde el panel de Jaltest Auth.",
        gated: true,
        aiDraft: true,
        resourceUrl: "https://video.internal/auth/revocar-tablet",
        durationMin: 5
      }
    ],
    myProgress: { completedModuleIds: [], examScores: {} },
    team: [
      { name: "Sara Molina", progress: 100 },
      { name: "David Prats", progress: 0 }
    ]
  },

  "onboarding-cloud": {
    id: "onboarding-cloud",
    title: "Onboarding Plataforma Cloud",
    audience: "Plataforma Cloud",
    description: "Formación guiada sobre aislamiento multi-departamento e infraestructura intercambiable.",
    status: "borrador",
    linkedChatbotKey: "cloud",
    modules: [
      {
        id: "cloud-m1",
        title: "Aislamiento por departamento: database-per-tenant",
        type: "documento",
        summary: "Por qué cada departamento tiene su propia base de datos y dónde se resuelve el tenant.",
        gated: false,
        aiDraft: false,
        resourceUrl: "https://docs.internal/cloud/stack-tecnico"
      },
      {
        id: "cloud-m2",
        title: "Examen: puertos e infraestructura intercambiable",
        type: "examen",
        summary: "Valida que entiendes por qué el dominio nunca llama directamente a pgvector.",
        gated: true,
        aiDraft: false,
        passingScore: 70,
        quiz: [
          {
            question: "¿Por qué cada departamento tiene su propia base de datos?",
            options: ["Por convención, sin motivo técnico", "Para aislamiento físico y evitar fugas por filtrado lógico", "Porque PostgreSQL lo exige", "Para ahorrar costes"],
            correctIndex: 1
          }
        ]
      }
    ],
    myProgress: { completedModuleIds: [], examScores: {} },
    team: [
      { name: "Pau Ferran", progress: 100 },
      { name: "Marta Vidal", progress: 50 }
    ]
  },

  "onboarding-empresa": {
    id: "onboarding-empresa",
    title: "Onboarding General de Empresa",
    audience: "Toda la empresa",
    description: "Formación transversal común a cualquier departamento: cultura, políticas internas y herramientas comunes.",
    status: "activo",
    linkedChatbotKey: "general",
    modules: [
      {
        id: "general-m1",
        title: "Vídeo: bienvenida a la empresa",
        type: "video",
        summary: "Estructura general, valores y primeros pasos comunes a cualquier departamento.",
        gated: false,
        aiDraft: false,
        resourceUrl: "https://video.internal/general/bienvenida",
        durationMin: 4
      },
      {
        id: "general-m2",
        title: "Herramientas y accesos comunes",
        type: "documento",
        summary: "VPN, correo, calendario y el resto de herramientas que usa toda la empresa.",
        gated: false,
        aiDraft: true,
        resourceUrl: "https://docs.internal/general/manual-it-empresa"
      },
      {
        id: "general-m3",
        title: "Examen: cultura y políticas internas",
        type: "examen",
        summary: "Comprueba lo esencial de las políticas comunes antes de entrar en la formación de tu departamento.",
        gated: true,
        aiDraft: false,
        passingScore: 70,
        quiz: [
          {
            question: "¿Dónde se solicita el acceso VPN?",
            options: ["Por Slack a un compañero", "Portal de IT interno", "No hace falta solicitarlo", "Por email a dirección"],
            correctIndex: 1
          },
          {
            question: "¿Quién aprueba las solicitudes de vacaciones?",
            options: ["RRHH directamente siempre", "Tu manager directo desde el portal de RRHH", "Se aprueban automáticamente", "El equipo de IT"],
            correctIndex: 1
          }
        ]
      }
    ],
    myProgress: { completedModuleIds: ["general-m1", "general-m2", "general-m3"], examScores: { "general-m3": 100 } },
    team: [
      { name: "RRHH — Recursos Humanos", progress: 100 },
      { name: "Iker Sola", progress: 100 }
    ]
  }
};

export class MockFormationsRepository implements IFormationsRepository {
  private data: FormationMap = structuredClone(seedFormations);

  async FindAll(): Promise<FormationMap> {
    return structuredClone(this.data);
  }

  async Create(input: CreateFormationInput): Promise<Formation> {
    const { title, audience, description, status, linkedChatbotKey, iconUrl } = input;
    const id = slugify(title, Object.keys(this.data));
    const trimmedDescription = description.trim();
    const newFormation: Formation = {
      id,
      title,
      audience: audience || "Sin definir",
      description: trimmedDescription || "Formación recién creada. Añade módulos para que el equipo pueda empezar a formarse.",
      status,
      linkedChatbotKey,
      iconUrl,
      modules: [],
      myProgress: { completedModuleIds: [], examScores: {} },
      team: []
    };
    this.data = { ...this.data, [id]: newFormation };
    return structuredClone(newFormation);
  }

  async Archive(key: string): Promise<void> {
    this.update(key, (c) => ({ ...c, archived: true }));
  }

  async UpdateStatus(key: string, status: Formation["status"]): Promise<void> {
    this.update(key, (c) => ({ ...c, status }));
  }

  async UpdateIcon(key: string, iconUrl: string | undefined): Promise<void> {
    this.update(key, (c) => ({ ...c, iconUrl }));
  }

  async AddModule(key: string, module: Omit<FormationModule, "id">): Promise<void> {
    this.update(key, (c) => {
      const id = slugify(module.title, c.modules.map((m) => m.id));
      return { ...c, modules: [...c.modules, { ...module, id }] };
    });
  }

  async RemoveModule(key: string, moduleId: string): Promise<void> {
    this.update(key, (c) => ({
      ...c,
      modules: c.modules.filter((m) => m.id !== moduleId),
      myProgress: {
        ...c.myProgress,
        completedModuleIds: c.myProgress.completedModuleIds.filter((id) => id !== moduleId)
      }
    }));
  }

  async ToggleModuleGating(key: string, moduleId: string): Promise<void> {
    this.update(key, (c) => ({
      ...c,
      modules: c.modules.map((m) => (m.id === moduleId ? { ...m, gated: !m.gated } : m))
    }));
  }

  async CompleteModule(key: string, moduleId: string, score?: number): Promise<void> {
    this.update(key, (c) => {
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

  private update(key: string, updater: (c: Formation) => Formation) {
    const c = this.data[key];
    if (!c) return;
    this.data = { ...this.data, [key]: updater(c) };
  }
}
