import type { LearnerProgress } from "./Formation";

export interface IFormationEnrollmentRepository {
  FindMyProgress(formationId: string): Promise<LearnerProgress | null>;
  FindTeamProgress(formationId: string): Promise<Record<string, LearnerProgress>>;
}
