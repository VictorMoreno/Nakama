type ExpiredListener = () => void;
type Refresher = () => Promise<boolean>;

let accessToken: string | null = null;
let refresher: Refresher | null = null;
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
  },

  setRefresher(value: Refresher): void {
    refresher = value;
  },

  async refresh(): Promise<boolean> {
    return refresher ? refresher() : false;
  }
};
