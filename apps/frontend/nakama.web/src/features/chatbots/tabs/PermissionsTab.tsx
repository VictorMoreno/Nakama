import { useState } from "react";
import { ConfirmDialog } from "../../../components/ConfirmDialog";
import { isOwnerRole, useChatbots } from "../../../context/ChatbotsContext";
import { useToast } from "../../../context/ToastContext";
import type { Chatbot } from "../../../types/chatbot";
import { InviteDialog } from "../InviteDialog";

interface PermissionsTabProps {
  bot: Chatbot;
}

function roleClass(role: string): string {
  if (role.startsWith("Propietari")) return "owner";
  if (role === "Editor") return "editor";
  return "viewer";
}

export function PermissionsTab({ bot }: PermissionsTabProps) {
  const { removeTeamMember } = useChatbots();
  const { showToast } = useToast();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [removeIndex, setRemoveIndex] = useState<number | null>(null);
  const target = removeIndex != null ? bot.team[removeIndex] : null;

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Personas con acceso</h2>
          <button className="btn-primary" type="button" onClick={() => setInviteOpen(true)}>
            + Invitar
          </button>
        </div>
        <div className="team-list">
          {bot.team.map((person, idx) => (
            <div className="team-item" key={`${person.name}-${idx}`}>
              <div>
                <span className="team-item-name">{person.name}</span>
                <span className={`role-badge ${roleClass(person.role)}`}>{person.role}</span>
              </div>
              {!isOwnerRole(person.role) && (
                <button className="btn-ghost danger" type="button" onClick={() => setRemoveIndex(idx)}>
                  Quitar acceso
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
        onConfirm={() => {
          if (removeIndex != null) {
            removeTeamMember(bot.key, removeIndex);
            showToast("Acceso actualizado");
          }
          setRemoveIndex(null);
        }}
      />
    </div>
  );
}
