import { useRef, useState } from "react";
import type { FormEvent } from "react";
import { Link, useLocation } from "react-router-dom";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import type { AuthRedirectState } from "../auth/guards";
import { validateEmail } from "../auth/validation";
import { AuthLayout } from "../components/AuthLayout";
import { Field } from "../components/Field";
import { Notice } from "../components/Notice";
import { errorMessage } from "../i18n/errors";
import { usePageTitle } from "./usePageTitle";

const DEMO_EMAIL = import.meta.env.VITE_DEMO_EMAIL;
const DEMO_PASSWORD = import.meta.env.VITE_DEMO_PASSWORD;
const DEMO_ENABLED = Boolean(DEMO_EMAIL && DEMO_PASSWORD);

interface Errors {
  email?: string;
  password?: string;
  form?: string;
}

export default function Login() {
  const { login, sessionExpired, accountDeleted } = useAuth();
  usePageTitle("Entrar");
  const state = useLocation().state as AuthRedirectState | null;
  const [email, setEmail] = useState(state?.registeredEmail ?? "");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<Errors>({});
  const [pending, setPending] = useState<"form" | "demo" | null>(null);
  const emailRef = useRef<HTMLInputElement>(null);
  const passwordRef = useRef<HTMLInputElement>(null);

  async function signIn(credentials: { email: string; password: string }, source: "form" | "demo") {
    setPending(source);
    setErrors({});
    try {
      await login(credentials.email, credentials.password);
      // RedirectIfAuthenticated takes the user to where they were going.
    } catch (error) {
      const code = error instanceof ApiError ? error.code : "UNKNOWN";
      setErrors({ form: errorMessage(code) });
      setPending(null);
      if (source === "form" && code === "INVALID_CREDENTIALS") passwordRef.current?.select();
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (pending) return;
    const next: Errors = {
      email: validateEmail(email),
      password: password ? undefined : "Informe sua senha.",
    };
    if (next.email || next.password) {
      setErrors(next);
      (next.email ? emailRef : passwordRef).current?.focus();
      return;
    }
    void signIn({ email: email.trim(), password }, "form");
  }

  return (
    <AuthLayout>
      <div className="auth-form__intro">
        <h1 className="auth-form__title">Entrar</h1>
        <p className="auth-form__lead">Veja como está o seu mês.</p>
      </div>

      {sessionExpired ? <Notice>Sua sessão expirou. Entre novamente para continuar.</Notice> : null}
      {accountDeleted ? <Notice>Sua conta e todos os seus dados foram excluídos.</Notice> : null}
      {state?.registeredEmail ? <Notice>Sua conta foi criada. Entre com seu email e senha.</Notice> : null}

      <form className="auth-form" onSubmit={handleSubmit} noValidate aria-busy={pending !== null}>
        <div className="live-region" aria-live="assertive">
          {errors.form ? <Notice tone="danger">{errors.form}</Notice> : null}
        </div>
        <Field
          ref={emailRef}
          label="Email"
          type="email"
          name="email"
          autoComplete="username"
          inputMode="email"
          spellCheck={false}
          value={email}
          onChange={(e) => {
            setEmail(e.target.value);
            if (errors.email) setErrors((x) => ({ ...x, email: undefined }));
          }}
          error={errors.email}
          required
        />
        <Field
          ref={passwordRef}
          label="Senha"
          type="password"
          name="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => {
            setPassword(e.target.value);
            if (errors.password) setErrors((x) => ({ ...x, password: undefined }));
          }}
          error={errors.password}
          revealable
          required
        />
        <button
          className="button button--primary button--block"
          type="submit"
          disabled={pending !== null}
          data-pending={pending === "form" || undefined}
        >
          {pending === "form" ? "Entrando…" : "Entrar"}
        </button>
      </form>

      {DEMO_ENABLED ? (
        <div className="auth-form__demo">
          <div className="auth-form__or" aria-hidden="true">
            <span>ou</span>
          </div>
          <button
            type="button"
            className="button button--secondary button--block"
            disabled={pending !== null}
            data-pending={pending === "demo" || undefined}
            onClick={() => void signIn({ email: DEMO_EMAIL!, password: DEMO_PASSWORD! }, "demo")}
            aria-describedby="demo-note"
          >
            {pending === "demo" ? "Entrando na conta demo…" : "Experimentar com a conta demo"}
          </button>
          <p className="auth-form__note" id="demo-note">
            Dados fictícios, compartilhados com outros visitantes.
          </p>
        </div>
      ) : null}

      <p className="auth-form__switch">
        Ainda não tem conta? <Link to="/register" state={state?.from ? { from: state.from } : undefined}>
          Criar conta
        </Link>
      </p>
    </AuthLayout>
  );
}
