import ConfirmDialog from "../../../components/shared/ConfirmDialog";
import { MailIcon, TrashIcon } from "../../../components/shared/icons";
import { isOwnerRole } from "../../../../modules/chatbots/domain/Chatbot";
import type { Chatbot } from "../../../../modules/chatbots/domain/Chatbot";
import InviteDialog from "../dialogs/InviteDialog";
import { usePermissionsTab } from "./hooks/usePermissionsTab";
import styles from "./PermissionsTab.module.scss";

interface PermissionsTabProps {
  bot: Chatbot;
}

function roleClass(role: string): string {
  if (role.startsWith("Propietari")) return "owner";
  if (role === "Editor") return "editor";
  return "viewer";
}

export default function PermissionsTab({ bot }: PermissionsTabProps) {
  const { inviteOpen, setInviteOpen, setRemoveIndex, target, handleRemoveConfirm } = usePermissionsTab(bot);

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Personas con acceso</h2>
          <button className="btn-primary" type="button" onClick={() => setInviteOpen(true)}>
            <MailIcon /> Invitar
          </button>
        </div>
        <div className={styles.teamList}>
          {bot.team.map((person, idx) => (
            <div className={styles.teamItem} key={`${person.name}-${idx}`}>
              <div>
                <span className={styles.teamItemName}>{person.name}</span>
                <span className={`${styles.roleBadge} ${styles[roleClass(person.role)]}`}>{person.role}</span>
              </div>
              {!isOwnerRole(person.role) && (
                <button className="btn-ghost danger" type="button" onClick={() => setRemoveIndex(idx)}>
                  <TrashIcon /> Quitar acceso
                </button>
              )}
            </div>
          ))}
        </div>
      </div>

      <InviteDialog open={inviteOpen} botKey={bot.key} botLabel={bot.label} onClose={() => setInviteOpen(false)} />

      <ConfirmDialog
        open={target != null}
        title="Quitar acceso"
        message={
          <>
            ¿Quitar acceso de <strong>{target?.name}</strong> a este chatbot?
          </>
        }
        confirmLabel="Quitar acceso"
        danger
        onCancel={() => setRemoveIndex(null)}
        onConfirm={handleRemoveConfirm}
      />
    </div>
  );
}
