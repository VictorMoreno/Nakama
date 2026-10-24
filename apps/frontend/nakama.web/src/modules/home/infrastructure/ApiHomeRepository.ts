import type { GeneratedNakamaClient, HomeResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { HomeSummary } from "../domain/Home";
import type { IHomeRepository } from "../domain/IHomeRepository";

function toHomeSummary(response: HomeResponse): HomeSummary {
  return {
    questionsThisWeek: response.questionsThisWeek ?? 0,
    activeDaysLast30: response.activeDaysLast30 ?? 0,
    formations: (response.formations ?? []).map((row) => ({
      formationId: row.formationId ?? "",
      title: row.title ?? "",
      progressPct: row.progressPct ?? 0,
      completedModules: row.completedModules ?? 0,
      modulesCount: row.modulesCount ?? 0,
      nextModuleTitle: row.nextModuleTitle,
      completed: row.completed ?? false
    })),
    pendingExams: (response.pendingExams ?? []).map((row) => ({
      formationId: row.formationId ?? "",
      formationTitle: row.formationTitle ?? "",
      moduleId: row.moduleId ?? "",
      moduleTitle: row.moduleTitle ?? "",
      lastScore: row.lastScore ?? 0,
      passingScore: row.passingScore ?? 0
    }))
  };
}

export class ApiHomeRepository implements IHomeRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async Find(): Promise<HomeSummary> {
    const response = await this.client.home();
    return toHomeSummary(response);
  }
}
