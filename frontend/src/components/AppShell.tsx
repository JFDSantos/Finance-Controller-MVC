import { useRef, useState } from "react";
import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { DeleteAccountDialog } from "./DeleteAccountDialog";
import { UserMenu } from "./UserMenu";
import { Wordmark } from "./Wordmark";

const NAV = [
  { to: "/", label: "Painel", end: true },
  { to: "/movimentacoes", label: "Movimentações", end: false },
  { to: "/categorias", label: "Categorias", end: false },
];

export function AppShell() {
  const { user, logout, deleteAccount } = useAuth();
  const navigate = useNavigate();
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const mainRef = useRef<HTMLElement>(null);
  const menuButtonRef = useRef<HTMLButtonElement>(null);

  if (!user) return null;

  async function handleLogout() {
    await logout().catch(() => undefined);
    navigate("/login", { replace: true });
  }

  async function handleDelete() {
    await deleteAccount();
    setConfirmingDelete(false);
    navigate("/login", { replace: true });
  }

  return (
    <div className="shell">
      <a
        className="skip-link"
        href="#conteudo"
        onClick={(event) => {
          event.preventDefault();
          mainRef.current?.focus();
        }}
      >
        Pular para o conteúdo
      </a>
      <header className="shell__header">
        <div className="shell__bar">
          <Link to="/" className="shell__brand" aria-label="Finance Controller, ir para o painel">
            <Wordmark className="wordmark--on-cobalt" />
          </Link>
          <nav className="shell__nav" aria-label="Principal">
            <ul>
              {NAV.map((item) => (
                <li key={item.to}>
                  <NavLink to={item.to} end={item.end} className="shell__nav-link">
                    {item.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>
          <UserMenu
            ref={menuButtonRef}
            user={user}
            onLogout={() => void handleLogout()}
            onDeleteAccount={() => setConfirmingDelete(true)}
          />
        </div>
      </header>
      <main id="conteudo" className="shell__main" ref={mainRef} tabIndex={-1}>
        <Outlet />
      </main>
      <DeleteAccountDialog
        open={confirmingDelete}
        email={user.email}
        onCancel={() => setConfirmingDelete(false)}
        returnFocusTo={menuButtonRef}
        onConfirm={handleDelete}
      />
    </div>
  );
}
