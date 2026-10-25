import { useMemo, useState } from "react";
import Dialog from "../../../components/shared/Dialog";
import Select from "../../../components/shared/Select";
import { CloseIcon, MailIcon } from "../../../components/shared/icons";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUsers } from "../../../shared/contexts/UsersContext";
import type { TeamRole } from "../../../../modules/chatbots/domain/Chatbot";

const ROLE_OPTIONS: { value: TeamRole; label: string }[] = [
  { value: "Editor", label: "Editor — puede editar documentos y configuración" },
  { value: "Lector", label: "Lector — solo puede consultar el chatbot" }
];

interface InviteDialogProps {
  open: boolean;
  botKey: string;
  botLabel: string;
  existingUserIds: string[];
  onClose: () => void;
}

export default function InviteDialog({ open, botKey, botLabel, existingUserIds, onClose }: InviteDialogProps) {
  const { users } = useUsers();
  const availableUsers = useMemo(
    () => users.filter((u) => !existingUserIds.includes(u.id)),
    [users, existingUserIds]
  );
  const [userId, setUserId] = useState("");
  const [role, setRole] = useState<TeamRole>("Lector");
  const [userError, setUserError] = useState(false);
  const { addTeamMember } = useChatbots();
  const { showToast } = useToast();

  const userOptions = availableUsers.map((u) => ({ value: u.id, label: `${u.name} (${u.email})` }));

  function reset() {
    setUserId("");
    setRole("Lector");
    setUserError(false);
  }

  function handleClose() {
    reset();
    onClose();
  }

  function handleConfirm() {
    setUserError(!userId);
    if (!userId) return;

    const user = availableUsers.find((u) => u.id === userId);
    addTeamMember(botKey, { userId, role });
    showToast(`${user?.name ?? "Persona"} añadida al chatbot`);
    reset();
    onClose();
  }

  return (
    <Dialog open={open} onClose={handleClose} title={`Invitar a ${botLabel}`}>
      <div className="form-field">
        <label htmlFor="invite-user">Persona</label>
        {userOptions.length === 0 ? (
          <p className="form-hint">Todos los usuarios del sistema ya tienen acceso a este chatbot.</p>
        ) : (
          <Select
            id="invite-user"
            value={userId}
            onChange={(v) => {
              setUserId(v);
              if (userError) setUserError(false);
            }}
            options={userOptions}
            ariaLabel="Persona"
            fullWidth
          />
        )}
      </div>
      <div className="form-field">
        <label htmlFor="invite-role">Rol</label>
        <Select id="invite-role" value={role} onChange={setRole} options={ROLE_OPTIONS} ariaLabel="Rol" fullWidth />
      </div>
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={handleClose}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" disabled={userOptions.length === 0} onClick={handleConfirm}>
          <MailIcon /> Invitar
        </button>
      </div>
    </Dialog>
  );
}
