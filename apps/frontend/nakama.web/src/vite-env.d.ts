/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_CHAT_MAX_ATTACHMENTS: string;
  readonly VITE_CHAT_MAX_ATTACHMENT_SIZE_MB: string;
  readonly VITE_CHAT_ALLOWED_ATTACHMENT_EXTENSIONS: string;
  readonly VITE_DOCUMENTS_MAX_SIZE_MB: string;
  readonly VITE_DOCUMENTS_ALLOWED_EXTENSIONS: string;
  readonly VITE_ICONS_MAX_SIZE_MB: string;
  readonly VITE_ICONS_ALLOWED_CONTENT_TYPES: string;
  readonly VITE_PAGINATION_DEFAULT_PAGE_SIZE: string;
  readonly VITE_PAGINATION_PAGE_SIZE_OPTIONS: string;
  readonly VITE_USER_ACTIVITY_DEFAULT_DAYS: string;
  readonly VITE_USER_ACTIVITY_DAY_OPTIONS: string;
  readonly VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS: string;
  readonly VITE_QUESTION_ACTIVITY_WEEK_OPTIONS: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
