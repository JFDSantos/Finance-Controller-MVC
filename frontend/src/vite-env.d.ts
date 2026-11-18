/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Public demo account email. The demo button shows only when both demo variables are set at build time. */
  readonly VITE_DEMO_EMAIL?: string;
  /** Public demo account password (the demo account is public by design). */
  readonly VITE_DEMO_PASSWORD?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
