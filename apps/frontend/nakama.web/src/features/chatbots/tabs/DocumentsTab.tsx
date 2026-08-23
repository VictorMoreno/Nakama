import { useState } from "react";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { useChatbots } from "../../../context/ChatbotsContext";
import { useToast } from "../../../context/ToastContext";
import type { Chatbot } from "../../../types/chatbot";
import { UploadDocumentDialog } from "../UploadDocumentDialog";

interface DocumentsTabProps {
  bot: Chatbot;
}

export function DocumentsTab({ bot }: DocumentsTabProps) {
  const { removeDocument } = useChatbots();
  const { showToast } = useToast();
  const [uploadOpen, setUploadOpen] = useState(false);
  const [deleteIndex, setDeleteIndex] = useState<number | null>(null);
  const target = deleteIndex != null ? bot.docs[deleteIndex] : null;

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Documentos</h2>
          <button className="btn-primary" type="button" onClick={() => setUploadOpen(true)}>
            + Subir documento
          </button>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Documento</th>
                <th>Estado</th>
                <th>Actualizado</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {bot.docs.map((doc, idx) => (
                <tr key={doc.name}>
                  <td className="doc-name">{doc.name}</td>
                  <td>
                    <span className={`status-tag ${doc.status}`}>{doc.statusLabel}</span>
                  </td>
                  <td>{doc.updated}</td>
                  <td>
                    <button className="btn-ghost danger" type="button" onClick={() => setDeleteIndex(idx)}>
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <UploadDocumentDialog open={uploadOpen} botLabel={bot.label} onClose={() => setUploadOpen(false)} />

      <ConfirmDialog
        open={target != null}
        title="Eliminar documento"
        message={
          <>
            ¿Eliminar <strong>{target?.name}</strong> de la base de conocimiento de este chatbot? El bot dejará de usarlo para responder.
          </>
        }
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteIndex(null)}
        onConfirm={() => {
          if (deleteIndex != null) {
            removeDocument(bot.key, deleteIndex);
            showToast("Documento eliminado");
          }
          setDeleteIndex(null);
        }}
      />
    </div>
  );
}
