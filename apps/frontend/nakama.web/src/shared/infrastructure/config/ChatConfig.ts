import { readList, readPositiveInt } from "./EnvReaders";

export const chatConfig = {
  maxAttachments: readPositiveInt("VITE_CHAT_MAX_ATTACHMENTS", import.meta.env.VITE_CHAT_MAX_ATTACHMENTS),
  maxAttachmentSizeMb: readPositiveInt("VITE_CHAT_MAX_ATTACHMENT_SIZE_MB", import.meta.env.VITE_CHAT_MAX_ATTACHMENT_SIZE_MB),
  allowedAttachmentExtensions: readList(
    "VITE_CHAT_ALLOWED_ATTACHMENT_EXTENSIONS",
    import.meta.env.VITE_CHAT_ALLOWED_ATTACHMENT_EXTENSIONS
  )
} as const;
