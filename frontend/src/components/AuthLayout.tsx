import type { ReactNode } from "react";
import { TilePanel } from "./TilePanel";
import { Wordmark } from "./Wordmark";

/** Sign-in and sign-up frame: a cobalt tile panel (a month of tiles on desktop, one week on phones) beside the form. */
export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="auth">
      <header className="auth__panel">
        <Wordmark className="wordmark--on-cobalt" />
        <TilePanel cols={7} rows={5} animate className="auth__tiles auth__tiles--month" />
        <TilePanel cols={7} rows={1} animate className="auth__tiles auth__tiles--week" />
        <p className="auth__tagline">Suas receitas e despesas, mês a mês.</p>
      </header>
      <main className="auth__main">
        <div className="auth__content">{children}</div>
      </main>
    </div>
  );
}
