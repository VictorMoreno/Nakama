import DocumentUploadDialog from "../../../components/shared/DocumentUploadDialog";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { documentsConfig } from "../../../../shared/infrastructure/config/DocumentsConfig";

interface UploadDocumentDialogProps {
  open: boolean;
  botId: string;
  botLabel: string;
  onClose: () => void;
}

export default function UploadDocumentDialog({ open, botId, botLabel, onClose }: UploadDocumentDialogProps) {
  const { addDocument } = useChatbots();

  return (
    <DocumentUploadDialog
      open={open}
      description={
        <>
          Sube un archivo ({documentsConfig.allowedExtensions.join(", ")}), máx. {documentsConfig.maxSizeMb} MB. Se añade a la base de
          conocimiento de <strong>{botLabel}</strong> — solo visible para este chatbot.
        </>
      }
      onUpload={(file) => addDocument(botId, file, file.name, "Subida manual")}
      onClose={onClose}
    />
  );
}
