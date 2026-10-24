import type { HomeSummary } from "./Home";

export interface IHomeRepository {
  Find(): Promise<HomeSummary>;
}
