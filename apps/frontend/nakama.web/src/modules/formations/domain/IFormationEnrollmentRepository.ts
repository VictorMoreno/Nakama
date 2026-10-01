import type { LearnerProgress } from "./Formation";

export interface IFormationEnrollmentRepository {
  FindMyProgress(formationId: string, userId: string): Promise<LearnerProgress | null>;
}
