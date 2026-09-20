/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base URL for GeneratedNakamaClient. Empty in dev (relative `/api/...`, proxied by vite.config.ts to the local API); set for a real deployment target when building for production. */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
