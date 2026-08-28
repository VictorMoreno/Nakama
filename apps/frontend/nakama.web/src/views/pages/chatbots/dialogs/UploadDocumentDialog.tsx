import Dialog from "../../../components/shared/Dialog";
import { useToast } from "../../../shared/contexts/ToastContext";

interface UploadDocumentDialogProps {
  open: boolean;
  botLabel: string;
  onClose: () => void;
}

export default function UploadDocumentDialog({ open, botLabel, onClose }: UploadDocumentDialogProps) {
  const { showToast } = useToast();

  return (
    <Dialog open={open} onClose={onClose} title="Subir documento">
      <p>
        Arrastra un PDF, Markdown o pega una URL de Confluence/Notion. Se indexa en <strong>{botLabel}</strong> — solo visible para este
        chatbot.
      </p>
      <div className="dropzone">Arrastra un archivo aquí, o haz clic para seleccionar</div>
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={onClose}>
          Cancelar
        </button>
        <button
          className="btn-primary"
          type="button"
          onClick={() => {
            onClose();
            showToast("Documento en cola de indexado");
          }}
        >
          Subir
        </button>
      </div>
    </Dialog>
  );
}
