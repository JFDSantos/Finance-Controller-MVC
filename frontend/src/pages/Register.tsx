import { useRef, useState } from "react";
import type { FormEvent, RefObject } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../api/client";
import { SignInAfterRegisterError, useAuth } from "../auth/AuthContext";
import type { AuthRedirectState } from "../auth/guards";
import { validateEmail, validateName, validateNewPassword } from "../auth/validation";
import { AuthLayout } from "../components/AuthLayout";
import { Field } from "../components/Field";
import { Notice } from "../components/Notice";
import { errorMessage, fieldErrorMessage } from "../i18n/errors";
import { usePageTitle } from "./usePageTitle";

type FieldName = "userName" | "email" | "password";
type Errors = Partial<Record<FieldName | "form", string>>;

const FIELD_ORDER: FieldName[] = ["userName", "email", "password"];

function serverErrors(error: unknown): Errors {
  if (!(error instanceof ApiError)) return { form: errorMessage("UNKNOWN") };
  if (error.code === "EMAIL_TAKEN") return { email: errorMessage("EMAIL_TAKEN") };
  if (error.code === "VALIDATION_FAILED" && error.fieldErrors.length > 0) {
    const result: Errors = {};
    for (const fieldError of error.fieldErrors) {
      const field = FIELD_ORDER.find((name) => name.toLowerCase() === fieldError.propertyName.toLowerCase());
      if (field && !result[field]) result[field] = fieldErrorMessage(field, fieldError.errorCode);
    }
    return Object.keys(result).length > 0 ? result : { form: errorMessage(error.code) };
  }
  return { form: errorMessage(error.code) };
}

export default function Register() {
  const { register } = useAuth();
  usePageTitle("Criar conta");
  const navigate = useNavigate();
  const [values, setValues] = useState({ userName: "", email: "", password: "" });
  const [errors, setErrors] = useState<Errors>({});
  const [pending, setPending] = useState(false);
  const refs: Record<FieldName, RefObject<HTMLInputElement | null>> = {
    userName: useRef<HTMLInputElement>(null),
    email: useRef<HTMLInputElement>(null),
    password: useRef<HTMLInputElement>(null),
  };

  function focusFirstInvalid(next: Errors) {
    const first = FIELD_ORDER.find((name) => next[name]);
    if (first) refs[first].current?.focus();
  }

  function update(field: FieldName, value: string) {
    setValues((v) => ({ ...v, [field]: value }));
    if (errors[field]) setErrors((e) => ({ ...e, [field]: undefined }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (pending) return;
    const next: Errors = {
      userName: validateName(values.userName),
      email: validateEmail(values.email),
      password: validateNewPassword(values.password),
    };
    if (FIELD_ORDER.some((name) => next[name])) {
      setErrors(next);
      focusFirstInvalid(next);
      return;
    }

    const dto = { userName: values.userName.trim(), email: values.email.trim(), password: values.password };
    setPending(true);
    setErrors({});
    try {
      await register(dto);
      navigate("/", { replace: true });
    } catch (error) {
      setPending(false);
      if (error instanceof SignInAfterRegisterError) {
        navigate("/login", { replace: true, state: { registeredEmail: dto.email } satisfies AuthRedirectState });
        return;
      }
      const mapped = serverErrors(error);
      setErrors(mapped);
      focusFirstInvalid(mapped);
    }
  }

  return (
    <AuthLayout>
      <div className="auth-form__intro">
        <h1 className="auth-form__title">Criar conta</h1>
        <p className="auth-form__lead">Leva menos de um minuto para começar.</p>
      </div>

      <form className="auth-form" onSubmit={handleSubmit} noValidate aria-busy={pending}>
        <div className="live-region" aria-live="assertive">
          {errors.form ? <Notice tone="danger">{errors.form}</Notice> : null}
        </div>
        <Field
          ref={refs.userName}
          label="Nome"
          name="userName"
          autoComplete="name"
          value={values.userName}
          onChange={(e) => update("userName", e.target.value)}
          error={errors.userName}
          maxLength={100}
          required
        />
        <Field
          ref={refs.email}
          label="Email"
          type="email"
          name="email"
          autoComplete="email"
          inputMode="email"
          spellCheck={false}
          value={values.email}
          onChange={(e) => update("email", e.target.value)}
          error={errors.email}
          required
        />
        <Field
          ref={refs.password}
          label="Senha"
          type="password"
          name="password"
          autoComplete="new-password"
          hint="Use pelo menos 8 caracteres."
          value={values.password}
          onChange={(e) => update("password", e.target.value)}
          error={errors.password}
          revealable
          required
        />
        <button className="button button--primary button--block" type="submit" disabled={pending}>
          {pending ? "Criando conta…" : "Criar conta"}
        </button>
      </form>

      <p className="auth-form__switch">
        Já tem conta? <Link to="/login">Entrar</Link>
      </p>
    </AuthLayout>
  );
}
