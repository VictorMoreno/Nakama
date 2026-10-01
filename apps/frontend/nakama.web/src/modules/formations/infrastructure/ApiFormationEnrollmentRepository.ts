import { GeneratedApiException, type GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { LearnerProgress } from "../domain/Formation";
import type { IFormationEnrollmentRepository } from "../domain/IFormationEnrollmentRepository";

export class ApiFormationEnrollmentRepository implements IFormationEnrollmentRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindMyProgress(formationId: string, userId: string): Promise<LearnerProgress | null> {
    try {
      const response = await this.client.enrollment(formationId, userId);
      return { completedModuleIds: response.completedModuleIds ?? [], examScores: response.examScores ?? {} };
    } catch (error) {
      if (error instanceof GeneratedApiException && error.status === 404) {
        return null;
      }
      throw error;
    }
  }
}
