import type { IUserRepository } from "../domain/IUserRepository";
import type { User } from "../domain/User";

const seedUser: User = {
  name: "Iker Sola",
  roleLine: "Lector · Diagnósticos ECU"
};

export class MockUserRepository implements IUserRepository {
  async GetCurrent(): Promise<User> {
    return structuredClone(seedUser);
  }
}
