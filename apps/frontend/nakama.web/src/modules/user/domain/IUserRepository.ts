import type { User } from "./User";

export interface IUserRepository {
  GetCurrent(): Promise<User>;
}
