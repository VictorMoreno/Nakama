import type { AuthenticatedUser } from "./AuthenticatedUser";

export interface IAuthRepository {
  Login(email: string, password: string): Promise<AuthenticatedUser | null>;
  Logout(): Promise<void>;
}
