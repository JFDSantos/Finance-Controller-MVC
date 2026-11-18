/**
 * The mark: four tiles whose quarter-discs meet in one disc, one quarter in ochre.
 * Built from the same tile module as the panels.
 */
function Mark() {
  return (
    <svg className="wordmark__mark" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      <rect className="wordmark__tile" x="0" y="0" width="11" height="11" />
      <rect className="wordmark__tile" x="13" y="0" width="11" height="11" />
      <rect className="wordmark__tile" x="0" y="13" width="11" height="11" />
      <rect className="wordmark__tile" x="13" y="13" width="11" height="11" />
      <path className="wordmark__disc" d="M11 11H0A11 11 0 0 1 11 0Z" />
      <path className="wordmark__disc" d="M13 11V0A11 11 0 0 1 24 11Z" />
      <path className="wordmark__disc" d="M11 13V24A11 11 0 0 1 0 13Z" />
      <path className="wordmark__disc wordmark__disc--accent" d="M13 13H24A11 11 0 0 1 13 24Z" />
    </svg>
  );
}

export function Wordmark({ className }: { className?: string }) {
  return (
    <span className={["wordmark", className ?? ""].join(" ").trim()}>
      <Mark />
      <span className="wordmark__name">Finance Controller</span>
    </span>
  );
}
