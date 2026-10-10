import { readList, readPositiveInt } from "./EnvReaders";

export const documentsConfig = {
  maxSizeMb: readPositiveInt("VITE_DOCUMENTS_MAX_SIZE_MB", import.meta.env.VITE_DOCUMENTS_MAX_SIZE_MB),
  allowedExtensions: readList("VITE_DOCUMENTS_ALLOWED_EXTENSIONS", import.meta.env.VITE_DOCUMENTS_ALLOWED_EXTENSIONS)
} as const;
