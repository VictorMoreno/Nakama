import { useMemo, useState } from "react";
import Dialog from "../../components/shared/Dialog";
import Select from "../../components/shared/Select";
import { CloseIcon, PlusIcon } from "../../components/shared/icons";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";
import { useUsers } from "../../shared/contexts/UsersContext";

interface AddTeamMemberDialogProps {
  open: boolean;
  formationKey: string;
  existingUserIds: string[];
  onClose: () => void;
}

export default function AddTeamMemberDialog({ open, formationKey, existingUserIds, onClose }: AddTeamMemberDialogProps) {
  const { users } = useUsers();
  const availableUsers = useMemo(
    () => users.filter((u) => !existingUserIds.includes(u.id)),
    [users, existingUserIds]
  );
  const [userId, setUserId] = useState("");
  const [userError, setUserError] = useState(false);
  const { addTeamMember } = useFormations();
  const { showToast } = useToast();

  const userOptions = availableUsers.map((u) => ({ value: u.id, label: `${u.name} (${u.email})` }));

  function reset() {
    setUserId("");
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
    addTeamMember(formationKey, userId);
    showToast(`${user?.name ?? "Persona"} añadida a la formación`);
    reset();
    onClose();
  }

  return (
    <Dialog open={open} onClose={handleClose} title="Asignar persona">
      <div className="form-field">
        <label htmlFor="team-member-user">Persona</label>
        {userOptions.length === 0 ? (
          <p className="form-hint">Todos los usuarios del sistema ya están asignados a esta formación.</p>
        ) : (
          <Select
            id="team-member-user"
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
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={handleClose}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" disabled={userOptions.length === 0} onClick={handleConfirm}>
          <PlusIcon /> Asignar
        </button>
      </div>
    </Dialog>
  );
}
