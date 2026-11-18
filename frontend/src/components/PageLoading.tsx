import { useEffect, useState } from "react";

/** Shown while the session is being restored. Stays blank for the first 300ms so fast answers do not flash. */
export function PageLoading() {
  const [visible, setVisible] = useState(false);
  useEffect(() => {
    const timer = window.setTimeout(() => setVisible(true), 300);
    return () => window.clearTimeout(timer);
  }, []);
  return (
    <div className="page-loading" role="status" aria-live="polite">
      {visible ? <span className="page-loading__text">Carregando…</span> : null}
    </div>
  );
}
