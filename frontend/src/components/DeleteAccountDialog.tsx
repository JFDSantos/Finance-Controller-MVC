import { useEffect, useId, useRef, useState } from "react";
import type { RefObject } from "react";
import { ApiError } from "../api/client";
import { errorMessage } from "../i18n/errors";
import { Notice } from "./Notice";

interface DeleteAccountDialogProps {
  open: boolean;
  email: string;
  onCancel(): void;
  onConfirm(): Promise<void>;
  /** Where focus goes after the dialog closes (the element that opened it may be gone). */
  returnFocusTo?: RefObject<HTMLElement | null>;
}

/** Native modal <dialog>: the browser keeps focus inside and maps Escape to cancel. */
export function DeleteAccountDialog({ open, email, onCancel, onConfirm, returnFocusTo }: DeleteAccountDialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const [acknowledged, setAcknowledged] = useState(false);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string>();
  const titleId = useId();
  const descriptionId = useId();
  const checkboxId = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (open && !dialog.open) {
      setAcknowledged(false);
      setError(undefined);
      if (typeof dialog.showModal === "function") dialog.showModal();
      else dialog.setAttribute("open", "");
    } else if (!open && dialog.open) {
      if (typeof dialog.close === "function") dialog.close();
      else dialog.removeAttribute("open");
      returnFocusTo?.current?.focus();
    }
  }, [open, returnFocusTo]);

  async function confirm() {
    setPending(true);
    setError(undefined);
    try {
      await onConfirm();
    } catch (cause) {
      setError(errorMessage(cause instanceof ApiError ? cause.code : "UNKNOWN"));
      setPending(false);
    }
  }

  return (
    <dialog
      ref={dialogRef}
      className="dialog"
      aria-labelledby={titleId}
      aria-describedby={descriptionId}
      onCancel={(event) => {
        event.preventDefault();
        if (!pending) onCancel();
      }}
    >
      <div className="dialog__body">
        <h2 className="dialog__title" id={titleId}>
          Excluir sua conta?
        </h2>
        <div id={descriptionId} className="dialog__text">
          <p>
            A conta <strong className="dialog__email">{email}</strong> será apagada permanentemente, junto com
            todas as suas categorias e movimentações.
          </p>
          <p>Não é possível desfazer nem recuperar esses dados depois.</p>
        </div>

        <div className="checkbox">
          <input
            id={checkboxId}
            type="checkbox"
            checked={acknowledged}
            onChange={(e) => setAcknowledged(e.target.checked)}
            disabled={pending}
          />
          <label htmlFor={checkboxId}>Entendo que meus dados serão apagados para sempre.</label>
        </div>

        <div className="live-region" aria-live="assertive">
          {error ? <Notice tone="danger">{error}</Notice> : null}
        </div>

        <div className="dialog__actions">
          <button type="button" className="button button--secondary" onClick={onCancel} disabled={pending}>
            Cancelar
          </button>
          <button
            type="button"
            className="button button--danger"
            onClick={() => void confirm()}
            disabled={!acknowledged || pending}
          >
            {pending ? "Excluindo…" : "Excluir conta"}
          </button>
        </div>
      </div>
    </dialog>
  );
}
