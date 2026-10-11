// Provider and hook are deliberately colocated (standard React Context convention) —
// this only costs Fast Refresh a full remount of consumers on edit, not correctness.
/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { AuthSession } from "../../../shared/infrastructure/auth/AuthSession";

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

export function UserProvider({ children }: { children: ReactNode }) {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);

  useEffect(() => AuthSession.onExpired(() => setCurrentUser(null)), []);

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
