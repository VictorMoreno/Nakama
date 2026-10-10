/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base URL for GeneratedNakamaClient. Empty in dev (relative `/api/...`, proxied by vite.config.ts to the local API); set for a real deployment target when building for production. */
  readonly VITE_API_BASE_URL?: string;
  /** Max files per chat message; mirrors Chat:MaxAttachmentsPerMessage in the backend appsettings.json. */
  readonly VITE_CHAT_MAX_ATTACHMENTS: string;
  /** Max size of each chat attachment in MB; mirrors Chat:MaxAttachmentSizeMb. */
  readonly VITE_CHAT_MAX_ATTACHMENT_SIZE_MB: string;
  /** Comma-separated allowed attachment extensions; mirrors Chat:AllowedAttachmentExtensions. */
  readonly VITE_CHAT_ALLOWED_ATTACHMENT_EXTENSIONS: string;
  /** Max size of a chatbot document in MB; mirrors Documents:MaxSizeMb. */
  readonly VITE_DOCUMENTS_MAX_SIZE_MB: string;
  /** Comma-separated allowed document extensions; mirrors Documents:AllowedExtensions. */
  readonly VITE_DOCUMENTS_ALLOWED_EXTENSIONS: string;
  /** Max size of an icon image in MB; mirrors Icons:MaxSizeMb. */
  readonly VITE_ICONS_MAX_SIZE_MB: string;
  /** Comma-separated allowed icon content types; mirrors the keys of Icons:ContentTypeExtensions. */
  readonly VITE_ICONS_ALLOWED_CONTENT_TYPES: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
