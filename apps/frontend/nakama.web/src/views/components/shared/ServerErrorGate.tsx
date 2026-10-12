import { useSyncExternalStore, type ReactNode } from "react";
import { ServerErrorSession } from "../../../shared/infrastructure/errors/ServerErrorSession";

interface ServerErrorGateProps {
  children: ReactNode;
}

export default function ServerErrorGate({ children }: ServerErrorGateProps) {
  const error = useSyncExternalStore(ServerErrorSession.subscribe, ServerErrorSession.getError);

  if (error) throw error;

  return children;
}
