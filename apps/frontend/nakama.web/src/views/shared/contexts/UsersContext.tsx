// Provider and hook are deliberately colocated (standard React Context convention) —
// this only costs Fast Refresh a full remount of consumers on edit, not correctness.
/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import type { UserSummary } from "../../../modules/users/domain/User";
import type { IUsersRepository } from "../../../modules/users/domain/IUsersRepository";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";

interface UsersContextValue {
  users: UserSummary[];
  isLoading: boolean;
  /** Cross-references a userId (e.g. a TeamMember row) against the loaded list - the display
   * name/email a picker or a team row shows, since neither is persisted on the assignment
   * itself anymore (it only stores the real UserId). */
  findUser: (userId: string) => UserSummary | undefined;
}

const UsersContext = createContext<UsersContextValue | null>(null);

export function UsersProvider({ children }: { children: ReactNode }) {
  const repo = useInjection<IUsersRepository>(Services.UsersRepository.id);
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    repo.FindAll().then((data) => {
      if (!cancelled) {
        setUsers(data);
        setIsLoading(false);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [repo]);

  function findUser(userId: string): UserSummary | undefined {
    return users.find((u) => u.id === userId);
  }

  const value: UsersContextValue = { users, isLoading, findUser };

  return <UsersContext.Provider value={value}>{children}</UsersContext.Provider>;
}

export function useUsers(): UsersContextValue {
  const ctx = useContext(UsersContext);
  if (!ctx) throw new Error("useUsers must be used within a UsersProvider");
  return ctx;
}
