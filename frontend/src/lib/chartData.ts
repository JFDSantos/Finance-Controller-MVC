import type { CategoryTotal, MonthPoint } from "../api/types";

/** Short pt-BR month names, as in "jan/26". */
export const SHORT_MONTHS = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"] as const;

/** At most this many category bars; past it the smallest fold into one "Outros" bar. */
export const MAX_SLICES = 8;
export const OTHERS_LABEL = "Outros";

export interface CategorySlice {
  name: string;
  value: number;
}

export interface SeriesPoint {
  label: string;
  income: number;
  expense: number;
}

/** Money to integer cents, so sums and comparisons never drift. */
export function toCents(value: number): number {
  return Math.round(value * 100);
}

export function fromCents(cents: number): number {
  return cents / 100;
}

const byName = new Intl.Collator("pt-BR", { sensitivity: "base", numeric: true });

/**
 * Where the month's expenses went: expense rows only, largest first, ties by name (pt-BR collation).
 * Rows at zero are dropped. Past MAX_SLICES the tail folds into one "Outros" slice, summed in cents, placed last.
 */
export function toCategorySlices(rows: CategoryTotal[]): CategorySlice[] {
  const expenses = rows
    .filter((row) => row.type === "Expense" && toCents(row.total) > 0)
    .map((row) => ({ name: row.categoryName, cents: toCents(row.total) }))
    .sort((a, b) => b.cents - a.cents || byName.compare(a.name, b.name));

  const kept = expenses.length > MAX_SLICES ? expenses.slice(0, MAX_SLICES - 1) : expenses;
  const slices = kept.map((s) => ({ name: s.name, value: fromCents(s.cents) }));
  if (expenses.length > MAX_SLICES) {
    const tail = expenses.slice(MAX_SLICES - 1).reduce((total, s) => total + s.cents, 0);
    slices.push({ name: OTHERS_LABEL, value: fromCents(tail) });
  }
  return slices;
}

/** (2026, 1) → "jan/26". */
export function shortMonthLabel(year: number, month: number): string {
  return `${SHORT_MONTHS[month - 1]}/${String(year % 100).padStart(2, "0")}`;
}

/** The API's months, in the order given (already chronological), with pt-BR short labels. */
export function toMonthlySeries(points: MonthPoint[]): SeriesPoint[] {
  return points.map((p) => ({
    label: shortMonthLabel(p.year, p.month),
    income: fromCents(toCents(p.income)),
    expense: fromCents(toCents(p.expense)),
  }));
}

/** Share of `part` in `whole` (0..1), computed in cents. 0 when the whole is 0. */
export function share(part: number, whole: number): number {
  const w = toCents(whole);
  return w === 0 ? 0 : toCents(part) / w;
}

/** Difference a − b without float drift. */
export function subtractMoney(a: number, b: number): number {
  return fromCents(toCents(a) - toCents(b));
}

/**
 * Clean axis ticks from 0 to at least `max`: steps of 1, 2, 2.5 or 5 × 10^k, about `count` intervals.
 * An all-zero series still gets a frame (0 to 100).
 */
export function niceTicks(max: number, count = 4): number[] {
  if (!(max > 0)) return [0, 25, 50, 75, 100];
  const raw = max / count;
  const magnitude = 10 ** Math.floor(Math.log10(raw));
  const step = [1, 2, 2.5, 5, 10].map((m) => m * magnitude).find((s) => s >= raw) ?? 10 * magnitude;
  const ticks: number[] = [];
  for (let v = 0; v < max + step * 0.999; v += step) ticks.push(Math.round(v * 100) / 100);
  return ticks;
}
