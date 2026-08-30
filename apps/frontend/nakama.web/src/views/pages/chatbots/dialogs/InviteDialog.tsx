import { useState } from "react";
import Dialog from "../../../components/shared/Dialog";
import Select from "../../../components/shared/Select";
import { CloseIcon, MailIcon } from "../../../components/shared/icons";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import type { TeamRole } from "../../../../modules/chatbots/domain/Chatbot";

const ROLE_OPTIONS: { value: TeamRole; label: string }[] = [
  { value: "Editor", label: "Editor — puede editar documentos y configuración" },
  { value: "Lector", label: "Lector — solo puede consultar el chatbot" }
];

interface InviteDialogProps {
  open: boolean;
  botKey: string;
  botLabel: string;
  onClose: () => void;
}

function isValidEmail(email: string): boolean {
  const at = email.indexOf("@");
  return at > 0 && email.indexOf(".", at) > -1;
}

export default function InviteDialog({ open, botKey, botLabel, onClose }: InviteDialogProps) {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<TeamRole>("Lector");
  const [nameError, setNameError] = useState(false);
  const [emailError, setEmailError] = useState(false);
  const { addTeamMember } = useChatbots();
  const { showToast } = useToast();

  function reset() {
    setName("");
    setEmail("");
    setRole("Lector");
    setNameError(false);
    setEmailError(false);
  }

  function handleClose() {
    reset();
    onClose();
  }

  function handleConfirm() {
    const trimmedName = name.trim();
    const trimmedEmail = email.trim();
    const validEmail = isValidEmail(trimmedEmail);
    setNameError(!trimmedName);
    setEmailError(!validEmail);
    if (!trimmedName || !validEmail) return;

    addTeamMember(botKey, { name: trimmedName, role });
    showToast(`Invitación enviada a ${trimmedEmail}`);
    reset();
    onClose();
  }

  return (
    <Dialog open={open} onClose={handleClose} title={`Invitar a ${botLabel}`}>
      <div className="form-field">
        <label htmlFor="invite-name">Nombre</label>
        <input
          id="invite-name"
          type="text"
          className={`form-input${nameError ? " field-error" : ""}`}
          placeholder="Nombre y apellido"
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            if (nameError) setNameError(false);
          }}
          autoFocus
        />
      </div>
      <div className="form-field">
        <label htmlFor="invite-email">Email</label>
        <input
          id="invite-email"
          type="email"
          className={`form-input${emailError ? " field-error" : ""}`}
          placeholder="nombre@empresa.com"
          value={email}
          onChange={(e) => {
            setEmail(e.target.value);
            if (emailError) setEmailError(false);
          }}
        />
      </div>
      <div className="form-field">
        <label htmlFor="invite-role">Rol</label>
        <Select id="invite-role" value={role} onChange={setRole} options={ROLE_OPTIONS} ariaLabel="Rol" fullWidth />
      </div>
      <p className="form-hint">
        <em>Modo demo: no se envía ningún email real.</em>
      </p>
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={handleClose}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" onClick={handleConfirm}>
          <MailIcon /> Invitar
        </button>
      </div>
    </Dialog>
  );
}
