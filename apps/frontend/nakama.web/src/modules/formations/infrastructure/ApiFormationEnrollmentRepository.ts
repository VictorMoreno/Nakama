import {
  GeneratedApiException,
  type FormationEnrollmentResponse,
  type GeneratedNakamaClient
} from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { LearnerProgress } from "../domain/Formation";
import type { IFormationEnrollmentRepository } from "../domain/IFormationEnrollmentRepository";

function toLearnerProgress(response: FormationEnrollmentResponse): LearnerProgress {
  return {
    completedModuleIds: response.completedModuleIds ?? [],
    examScores: response.examScores ?? {},
    examAttempts: (response.examAttempts ?? []).map((attempt) => ({
      moduleId: attempt.moduleId ?? "",
      score: attempt.score ?? 0,
      passed: attempt.passed ?? false,
      attemptedAt: attempt.attemptedAt ? new Date(attempt.attemptedAt).toISOString() : ""
    }))
  };
}

export class ApiFormationEnrollmentRepository implements IFormationEnrollmentRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindMyProgress(formationId: string): Promise<LearnerProgress | null> {
    try {
      const response = await this.client.enrollment(formationId);
      return toLearnerProgress(response);
    } catch (error) {
      if (error instanceof GeneratedApiException && error.status === 404) {
        return null;
      }
      throw error;
    }
  }

  async FindTeamProgress(formationId: string): Promise<Record<string, LearnerProgress>> {
    const rows = await this.client.enrollments(formationId);
    return Object.fromEntries(rows.map((row) => [row.userId ?? "", toLearnerProgress(row)]));
  }
}
