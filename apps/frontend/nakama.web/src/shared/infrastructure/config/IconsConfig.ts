import { readList, readPositiveInt } from "./EnvReaders";

export const iconsConfig = {
  maxSizeMb: readPositiveInt("VITE_ICONS_MAX_SIZE_MB", import.meta.env.VITE_ICONS_MAX_SIZE_MB),
  allowedContentTypes: readList("VITE_ICONS_ALLOWED_CONTENT_TYPES", import.meta.env.VITE_ICONS_ALLOWED_CONTENT_TYPES)
} as const;
