import { useId } from "react";
import { MONTH_NAMES, shiftMonth } from "../lib/format";
import { Icon } from "./Icon";
import { SelectField } from "./SelectField";

interface MonthFilterProps {
  year: number;
  month: number;
  onChange(year: number, month: number): void;
  /** The real current month, to offer a way back to it. */
  current: { year: number; month: number };
}

/**
 * Previous / next month buttons around a month and a year select. Two selects instead of
 * <input type="month">, which Safari and Firefox on desktop still render as a plain text box.
 */
export function MonthFilter({ year, month, onChange, current }: MonthFilterProps) {
  const labelId = useId();
  const first = Math.min(current.year - 10, year);
  const last = Math.max(current.year + 1, year);
  const years = Array.from({ length: last - first + 1 }, (_, i) => last - i);
  const isCurrent = year === current.year && month === current.month;

  function go(delta: number) {
    const next = shiftMonth(year, month, delta);
    onChange(next.year, next.month);
  }

  return (
    <div className="month-filter" role="group" aria-labelledby={labelId}>
      <div className="month-filter__head">
        <span className="field__label" id={labelId}>
          Período
        </span>
        {isCurrent ? null : (
          <button type="button" className="link-button" onClick={() => onChange(current.year, current.month)}>
            Voltar ao mês atual
          </button>
        )}
      </div>
      <div className="month-filter__controls">
        <button type="button" className="icon-button icon-button--framed" aria-label="Mês anterior" onClick={() => go(-1)}>
          <Icon name="chevronLeft" />
        </button>
        <SelectField
          label="Mês"
          hideLabel
          className="month-filter__month"
          value={String(month)}
          onChange={(event) => onChange(year, Number(event.target.value))}
        >
          {MONTH_NAMES.map((name, index) => (
            <option key={name} value={index + 1}>
              {name}
            </option>
          ))}
        </SelectField>
        <SelectField
          label="Ano"
          hideLabel
          className="month-filter__year"
          value={String(year)}
          onChange={(event) => onChange(Number(event.target.value), month)}
        >
          {years.map((y) => (
            <option key={y} value={y}>
              {y}
            </option>
          ))}
        </SelectField>
        <button type="button" className="icon-button icon-button--framed" aria-label="Próximo mês" onClick={() => go(1)}>
          <Icon name="chevronRight" />
        </button>
      </div>
    </div>
  );
}
