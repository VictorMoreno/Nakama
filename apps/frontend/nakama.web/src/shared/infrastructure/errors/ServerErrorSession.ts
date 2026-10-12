import type { ApiServerError } from "./ApiServerError";

type Listener = () => void;

let currentError: ApiServerError | null = null;
const listeners = new Set<Listener>();

export const ServerErrorSession = {
  getError(): ApiServerError | null {
    return currentError;
  },

  report(error: ApiServerError): void {
    if (currentError) return;
    currentError = error;
    listeners.forEach((listener) => listener());
  },

  subscribe(listener: Listener): () => void {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  }
};
