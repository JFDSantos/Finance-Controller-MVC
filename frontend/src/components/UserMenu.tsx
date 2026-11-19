import { forwardRef, useEffect, useId, useImperativeHandle, useRef, useState } from "react";
import type { User } from "../api/types";
import { Icon } from "./Icon";

interface UserMenuProps {
  user: User;
  onLogout(): void;
  onDeleteAccount(): void;
}

/** Shared public demo account (same address as DemoSeeder.DemoEmail); it cannot be deleted. */
const DEMO_EMAIL = "demo@financecontroller.app";

function initials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  const letters = parts.length > 1 ? parts[0][0] + parts[parts.length - 1][0] : (parts[0]?.[0] ?? "?");
  return letters.toLocaleUpperCase("pt-BR");
}

/** Disclosure menu: Escape or a click outside closes it and returns focus to the button. */
export const UserMenu = forwardRef<HTMLButtonElement | null, UserMenuProps>(function UserMenu(
  { user, onLogout, onDeleteAccount },
  forwardedRef,
) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);
  useImperativeHandle(forwardedRef, () => buttonRef.current!, []);
  const panelId = useId();

  useEffect(() => {
    if (!open) return;
    function onPointerDown(event: PointerEvent) {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    }
    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
        buttonRef.current?.focus();
      }
    }
    document.addEventListener("pointerdown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("pointerdown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  function choose(action: () => void) {
    setOpen(false);
    action();
  }

  return (
    <div
      className="user-menu"
      ref={rootRef}
      onBlur={(event) => {
        // Close only when focus is known to have moved elsewhere. A null relatedTarget is what Safari and
        // Firefox on macOS report when a mouse click lands on a button (they do not focus it); closing
        // then would hide the item before its click arrives. Outside clicks are handled by pointerdown.
        const next = event.relatedTarget as Node | null;
        if (open && next && !rootRef.current?.contains(next)) setOpen(false);
      }}
    >
      <button
        ref={buttonRef}
        type="button"
        className="user-menu__button"
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((v) => !v)}
      >
        <span className="user-menu__avatar" aria-hidden="true">
          {initials(user.userName)}
        </span>
        <span className="user-menu__name">
          <span className="visually-hidden">Conta de </span>
          {user.userName}
        </span>
        <Icon name="chevronDown" size={18} />
      </button>
      <div className="user-menu__panel" id={panelId} hidden={!open}>
        <div className="user-menu__identity">
          <p className="user-menu__identity-name">{user.userName}</p>
          <p className="user-menu__identity-email">{user.email}</p>
        </div>
        <ul className="user-menu__list">
          <li>
            <button type="button" className="user-menu__item" onClick={() => choose(onLogout)}>
              <Icon name="logOut" size={18} />
              Sair
            </button>
          </li>
          {user.email.toLowerCase() === DEMO_EMAIL ? null : (
            <li>
              <button
                type="button"
                className="user-menu__item user-menu__item--danger"
                onClick={() => choose(onDeleteAccount)}
              >
                <Icon name="trash" size={18} />
                Excluir minha conta
              </button>
            </li>
          )}
        </ul>
      </div>
    </div>
  );
});
