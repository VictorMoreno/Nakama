import ConfirmDialog from "../../../components/shared/ConfirmDialog";
import { TrashIcon, UploadIcon } from "../../../components/shared/icons";
import type { Chatbot } from "../../../../modules/chatbots/domain/Chatbot";
import UploadDocumentDialog from "../dialogs/UploadDocumentDialog";
import { useDocumentsTab } from "./hooks/useDocumentsTab";
import styles from "./DocumentsTab.module.scss";

interface DocumentsTabProps {
  bot: Chatbot;
}

export default function DocumentsTab({ bot }: DocumentsTabProps) {
  const { uploadOpen, setUploadOpen, setDeleteIndex, target, handleRemoveConfirm } = useDocumentsTab(bot);

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Documentos</h2>
          <button className="btn-primary" type="button" onClick={() => setUploadOpen(true)}>
            <UploadIcon /> Subir documento
          </button>
        </div>
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th className={styles.th}>Documento</th>
                <th className={styles.th}>Estado</th>
                <th className={styles.th}>Actualizado</th>
                <th className={styles.th}></th>
              </tr>
            </thead>
            <tbody>
              {bot.docs.map((doc, idx) => (
                <tr key={doc.id ?? doc.name}>
                  <td className={`${styles.td} ${styles.docName}`}>
                    {doc.url ? (
                      <a href={doc.url} target="_blank" rel="noreferrer">
                        {doc.name}
                      </a>
                    ) : (
                      doc.name
                    )}
                  </td>
                  <td className={styles.td}>
                    <span className={`status-tag ${doc.status}`}>{doc.statusLabel}</span>
                  </td>
                  <td className={styles.td}>{doc.updated}</td>
                  <td className={styles.td}>
                    <button className="btn-ghost danger" type="button" onClick={() => setDeleteIndex(idx)}>
                      <TrashIcon /> Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <UploadDocumentDialog open={uploadOpen} botId={bot.id} botLabel={bot.label} onClose={() => setUploadOpen(false)} />

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
        onConfirm={handleRemoveConfirm}
      />
    </div>
  );
}
