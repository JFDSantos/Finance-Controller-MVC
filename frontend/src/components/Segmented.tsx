import { useId } from "react";

export interface SegmentedOption<T extends string> {
  value: T;
  label: string;
}

interface SegmentedProps<T extends string> {
  legend: string;
  options: SegmentedOption<T>[];
  value: T;
  onChange(value: T): void;
  disabled?: boolean;
  hint?: string;
  className?: string;
}

/** A radio group drawn as a row of joined buttons. Arrow keys move between options (native radio behaviour). */
export function Segmented<T extends string>({
  legend,
  options,
  value,
  onChange,
  disabled,
  hint,
  className,
}: SegmentedProps<T>) {
  const name = useId();
  const hintId = `${name}-hint`;
  return (
    <fieldset
      className={["segmented", className ?? ""].join(" ").trim()}
      disabled={disabled}
      aria-describedby={hint ? hintId : undefined}
    >
      <legend className="field__label">{legend}</legend>
      <div className="segmented__options">
        {options.map((option) => (
          <label key={option.value} className="segmented__option">
            <input
              type="radio"
              name={name}
              value={option.value}
              checked={value === option.value}
              onChange={() => onChange(option.value)}
            />
            <span>{option.label}</span>
          </label>
        ))}
      </div>
      {hint ? (
        <p className="field__hint" id={hintId}>
          {hint}
        </p>
      ) : null}
    </fieldset>
  );
}
