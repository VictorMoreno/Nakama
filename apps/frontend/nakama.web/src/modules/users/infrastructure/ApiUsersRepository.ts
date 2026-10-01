import type { GeneratedNakamaClient, UserSummaryResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { UserSummary } from "../domain/User";
import type { IUsersRepository } from "../domain/IUsersRepository";

function toUserSummary(row: UserSummaryResponse): UserSummary {
  return { id: row.id ?? "", name: row.name ?? "", email: row.email ?? "" };
}

export class ApiUsersRepository implements IUsersRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<UserSummary[]> {
    const rows = await this.client.usersAll();
    return rows.map(toUserSummary);
  }
}
