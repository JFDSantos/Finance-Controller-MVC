import { useEffect, useId, useRef } from "react";
import type { ReactNode } from "react";

interface DialogProps {
  open: boolean;
  title: string;
  /** Escape or the cancel path. Ignored while `busy`. */
  onClose(): void;
  busy?: boolean;
  /** Where focus goes on close when the element that opened the dialog is gone (a deleted row). */
  fallbackFocus?: () => HTMLElement | null | undefined;
  describedBy?: string;
  wide?: boolean;
  children: ReactNode;
}

/**
 * Native modal <dialog>, as in DeleteAccountDialog: the browser traps focus and maps Escape to
 * cancel. Focus returns to whatever opened it, or to `fallbackFocus` when that element is gone.
 */
export function Dialog({ open, title, onClose, busy, fallbackFocus, describedBy, wide, children }: DialogProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const openerRef = useRef<HTMLElement | null>(null);
  const fallbackRef = useRef(fallbackFocus);
  fallbackRef.current = fallbackFocus;
  const titleId = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (open && !dialog.open) {
      openerRef.current = document.activeElement instanceof HTMLElement ? document.activeElement : null;
      if (typeof dialog.showModal === "function") dialog.showModal();
      else dialog.setAttribute("open", "");
    } else if (!open && dialog.open) {
      if (typeof dialog.close === "function") dialog.close();
      else dialog.removeAttribute("open");
      const opener = openerRef.current;
      // Let React commit list changes first, so a deleted row's button really is gone.
      queueMicrotask(() => {
        if (opener?.isConnected) opener.focus();
        else fallbackRef.current?.()?.focus();
      });
    }
  }, [open]);

  return (
    <dialog
      ref={dialogRef}
      className={wide ? "dialog dialog--wide" : "dialog"}
      aria-labelledby={titleId}
      aria-describedby={describedBy}
      onCancel={(event) => {
        event.preventDefault();
        if (!busy) onClose();
      }}
    >
      {open ? (
        <div className="dialog__body">
          <h2 className="dialog__title" id={titleId}>
            {title}
          </h2>
          {children}
        </div>
      ) : null}
    </dialog>
  );
}
