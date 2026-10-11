type ExpiredListener = () => void;

let accessToken: string | null = null;
const expiredListeners = new Set<ExpiredListener>();

export const AuthSession = {
  getAccessToken(): string | null {
    return accessToken;
  },

  setAccessToken(token: string): void {
    accessToken = token;
  },

  clear(): void {
    accessToken = null;
  },

  expire(): void {
    accessToken = null;
    expiredListeners.forEach((listener) => listener());
  },

  onExpired(listener: ExpiredListener): () => void {
    expiredListeners.add(listener);
    return () => {
      expiredListeners.delete(listener);
    };
  }
};
