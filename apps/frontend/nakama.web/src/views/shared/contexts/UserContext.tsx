// Provider and hook are deliberately colocated (standard React Context convention) —
// this only costs Fast Refresh a full remount of consumers on edit, not correctness.
/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import type { IAuthRepository } from "../../../modules/auth/domain/IAuthRepository";
import { AuthSession } from "../../../shared/infrastructure/auth/AuthSession";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";

interface CurrentUser {
  id: string;
  email: string;
  name: string;
  isAdmin: boolean;
}

interface UserContextValue {
  currentUser: CurrentUser | null;
  setCurrentUser: (user: CurrentUser | null) => void;
  isAdmin: boolean;
  isRestoring: boolean;
}

const UserContext = createContext<UserContextValue | null>(null);

export function UserProvider({ children }: { children: ReactNode }) {
  const authRepo = useInjection<IAuthRepository>(Services.AuthRepository.id);
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [isRestoring, setIsRestoring] = useState(true);

  useEffect(() => AuthSession.onExpired(() => setCurrentUser(null)), []);

  useEffect(() => {
    let active = true;
    authRepo
      .Refresh()
      .catch(() => null)
      .then((user) => {
        if (!active) return;
        setCurrentUser(user);
        setIsRestoring(false);
      });
    return () => {
      active = false;
    };
  }, [authRepo]);

  const value: UserContextValue = {
    currentUser,
    setCurrentUser,
    isAdmin: currentUser?.isAdmin ?? false,
    isRestoring
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
}

export function useUser(): UserContextValue {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error("useUser must be used within a UserProvider");
  return ctx;
}
