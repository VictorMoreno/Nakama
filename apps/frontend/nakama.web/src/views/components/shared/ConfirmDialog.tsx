import type { ReactNode } from "react";
import Dialog from "./Dialog";

interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: ReactNode;
  confirmLabel: string;
  danger?: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export default function ConfirmDialog({ open, title, message, confirmLabel, danger, onCancel, onConfirm }: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={onCancel} title={title}>
      <p>{message}</p>
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={onCancel}>
          Cancelar
        </button>
        <button className={`btn-primary${danger ? " danger" : ""}`} type="button" onClick={onConfirm}>
          {confirmLabel}
        </button>
      </div>
    </Dialog>
  );
}
