// Provider and hook are deliberately colocated (standard React Context convention) —
// this only costs Fast Refresh a full remount of consumers on edit, not correctness.
/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useState, type ReactNode } from "react";

export interface CurrentUser {
  id: string;
  email: string;
  name: string;
  isAdmin: boolean;
}

interface UserContextValue {
  currentUser: CurrentUser | null;
  setCurrentUser: (user: CurrentUser | null) => void;
  isAdmin: boolean;
}

const UserContext = createContext<UserContextValue | null>(null);

// Not repository-backed like ChatbotsContext/FormationsContext - there is nothing to fetch.
// currentUser is only ever written by LoginPage after a real POST /api/auth/login, and lives
// purely in memory: no session/cookie exists yet (ADR-20), so a page reload loses it, same as
// today's login already behaves.
export function UserProvider({ children }: { children: ReactNode }) {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);

  const value: UserContextValue = {
    currentUser,
    setCurrentUser,
    isAdmin: currentUser?.isAdmin ?? false
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
}

export function useUser(): UserContextValue {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error("useUser must be used within a UserProvider");
  return ctx;
}
