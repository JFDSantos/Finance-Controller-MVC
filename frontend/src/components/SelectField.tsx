import { forwardRef, useId } from "react";
import type { ReactNode, SelectHTMLAttributes } from "react";
import { Icon } from "./Icon";

interface SelectFieldProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, "id"> {
  label: string;
  /** Keeps the label for assistive tech but hides it on screen (the control sits in a labelled group). */
  hideLabel?: boolean;
  hint?: string;
  error?: string;
  children: ReactNode;
}

/** A native <select> dressed like `Field`: same edge, height, focus, error and hint wiring. */
export const SelectField = forwardRef<HTMLSelectElement, SelectFieldProps>(function SelectField(
  { label, hideLabel, hint, error, className, children, ...select },
  ref,
) {
  const id = useId();
  const hintId = `${id}-hint`;
  const errorId = `${id}-error`;
  const describedBy = error ? errorId : hint ? hintId : undefined;

  return (
    <div className={["field", error ? "field--invalid" : "", className ?? ""].join(" ").trim()}>
      <label className={hideLabel ? "visually-hidden" : "field__label"} htmlFor={id}>
        {label}
      </label>
      <div className="field__control field__control--select">
        <select
          ref={ref}
          id={id}
          className="field__input field__select"
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          {...select}
        >
          {children}
        </select>
        <span className="field__chevron" aria-hidden="true">
          <Icon name="chevronDown" size={18} />
        </span>
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
