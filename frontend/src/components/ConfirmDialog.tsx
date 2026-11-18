import { useEffect, useId, useState } from "react";
import type { ReactNode } from "react";
import { ApiError } from "../api/client";
import { errorMessage } from "../i18n/errors";
import { Dialog } from "./Dialog";
import { Notice } from "./Notice";

interface ConfirmDialogProps {
  open: boolean;
  title: string;
  /** What exactly will be lost. */
  children: ReactNode;
  confirmLabel: string;
  pendingLabel: string;
  onConfirm(): Promise<void>;
  onCancel(): void;
  /** Error codes after which trying again cannot help: the confirm button goes away and Cancel reads "Entendi". */
  finalErrors?: string[];
  fallbackFocus?: () => HTMLElement | null | undefined;
}

/** A destructive confirmation: Cancel first (it gets initial focus), the red action last, server errors in place. */
export function ConfirmDialog({
  open,
  title,
  children,
  confirmLabel,
  pendingLabel,
  onConfirm,
  onCancel,
  finalErrors = [],
  fallbackFocus,
}: ConfirmDialogProps) {
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<{ code: string; message: string }>();
  const descriptionId = useId();

  useEffect(() => {
    if (open) {
      setPending(false);
      setError(undefined);
    }
  }, [open]);

  async function confirm() {
    setPending(true);
    setError(undefined);
    try {
      await onConfirm();
    } catch (cause) {
      const code = cause instanceof ApiError ? cause.code : "UNKNOWN";
      setError({ code, message: errorMessage(code) });
    } finally {
      setPending(false);
    }
  }

  const blocked = error ? finalErrors.includes(error.code) : false;

  return (
    <Dialog
      open={open}
      title={title}
      onClose={onCancel}
      busy={pending}
      describedBy={descriptionId}
      fallbackFocus={fallbackFocus}
    >
      <div id={descriptionId} className="dialog__text">
        {children}
      </div>

      <div className="live-region" aria-live="assertive">
        {error ? <Notice tone="danger">{error.message}</Notice> : null}
      </div>

      <div className="dialog__actions">
        <button type="button" className="button button--secondary" onClick={onCancel} disabled={pending}>
          {blocked ? "Entendi" : "Cancelar"}
        </button>
        {blocked ? null : (
          <button
            type="button"
            className="button button--danger"
            onClick={() => void confirm()}
            disabled={pending}
            data-pending={pending || undefined}
          >
            {pending ? pendingLabel : confirmLabel}
          </button>
        )}
      </div>
    </Dialog>
  );
}
