import type { UserSummary } from "./User";

export interface IUsersRepository {
  FindAll(): Promise<UserSummary[]>;
}
