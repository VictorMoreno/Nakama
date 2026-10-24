import ConfirmDialog from "../../components/shared/ConfirmDialog";
import DocumentUploadDialog from "../../components/shared/DocumentUploadDialog";
import { RecycleBinIcon, UploadIcon } from "../../components/shared/icons";
import type { Formation } from "../../../modules/formations/domain/Formation";
import { documentsConfig } from "../../../shared/infrastructure/config/DocumentsConfig";
import { useFormationDocuments } from "./hooks/useFormationDocuments";
import styles from "./FormationDocumentsSection.module.scss";

interface FormationDocumentsSectionProps {
  formation: Formation;
}

export default function FormationDocumentsSection({ formation }: FormationDocumentsSectionProps) {
  const { uploadOpen, setUploadOpen, setDeleteId, target, handleUpload, handleRemoveConfirm, openDocument } =
    useFormationDocuments(formation);

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Documentos</h2>
          <button className="btn-primary" type="button" onClick={() => setUploadOpen(true)}>
            <UploadIcon /> Subir documento
          </button>
        </div>
        {formation.docs.length === 0 ? (
          <p className={styles.empty}>Todavía no hay documentos. Súbelos para poder generar preguntas de examen con IA a partir de ellos.</p>
        ) : (
          <div className={styles.tableWrap}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th className={styles.th}>Documento</th>
                  <th className={styles.th}>Actualizado</th>
                  <th className={styles.th}></th>
                </tr>
              </thead>
              <tbody>
                {formation.docs.map((doc) => (
                  <tr key={doc.id}>
                    <td className={`${styles.td} ${styles.docName}`}>
                      <button className={styles.docLink} type="button" onClick={() => openDocument(doc)}>
                        {doc.name}
                      </button>
                    </td>
                    <td className={styles.td}>{doc.updated}</td>
                    <td className={styles.td}>
                      <button className="btn-ghost danger" type="button" onClick={() => setDeleteId(doc.id)}>
                        <RecycleBinIcon /> Eliminar
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <DocumentUploadDialog
        open={uploadOpen}
        description={
          <>
            Sube un archivo ({documentsConfig.allowedExtensions.join(", ")}), máx. {documentsConfig.maxSizeMb} MB. Sirve para generar
            preguntas de examen con IA, y las personas que cursan la formación no lo ven.
          </>
        }
        onUpload={handleUpload}
        onClose={() => setUploadOpen(false)}
      />

      <ConfirmDialog
        open={target != null}
        title="Eliminar documento"
        message={
          <>
            ¿Eliminar <strong>{target?.name}</strong> de esta formación? Ya no se podrá usar para generar preguntas.
          </>
        }
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteId(null)}
        onConfirm={handleRemoveConfirm}
      />
    </div>
  );
}
