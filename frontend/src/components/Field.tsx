import { forwardRef, useId, useState } from "react";
import type { InputHTMLAttributes } from "react";
import { Icon } from "./Icon";

interface FieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "id"> {
  label: string;
  /** Helper text shown until the field has an error. */
  hint?: string;
  error?: string;
  /** Adds a show/hide toggle for password inputs. */
  revealable?: boolean;
}

export const Field = forwardRef<HTMLInputElement, FieldProps>(function Field(
  { label, hint, error, revealable, type = "text", className, ...input },
  ref,
) {
  const id = useId();
  const hintId = `${id}-hint`;
  const errorId = `${id}-error`;
  const [revealed, setRevealed] = useState(false);
  // The error replaces the hint, so assistive tech hears one description at a time.
  const describedBy = error ? errorId : hint ? hintId : undefined;

  return (
    <div className={["field", error ? "field--invalid" : "", className ?? ""].join(" ").trim()}>
      <label className="field__label" htmlFor={id}>
        {label}
      </label>
      <div className="field__control">
        <input
          ref={ref}
          id={id}
          className="field__input"
          type={revealable && revealed ? "text" : type}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          {...input}
        />
        {revealable ? (
          <button
            type="button"
            className="field__reveal"
            onClick={() => setRevealed((v) => !v)}
            aria-pressed={revealed}
            aria-label={revealed ? "Ocultar senha" : "Mostrar senha"}
            aria-controls={id}
          >
            <Icon name={revealed ? "eyeOff" : "eye"} />
          </button>
        ) : null}
      </div>
      {error ? (
        <p className="field__error" id={errorId}>
          <Icon name="alert" size={16} />
          <span>{error}</span>
        </p>
      ) : null}
      {hint && !error ? (
        <p className="field__hint" id={hintId}>
          {hint}
        </p>
      ) : null}
    </div>
  );
});
