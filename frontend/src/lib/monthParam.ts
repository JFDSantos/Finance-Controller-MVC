export interface YearMonth {
  year: number;
  month: number;
}

/** Reads `?mes=2028-01`; anything missing or invalid falls back to `fallback`. */
export function readMonthParam(params: URLSearchParams, fallback: YearMonth): YearMonth {
  const period = /^(\d{4})-(\d{2})$/.exec(params.get("mes") ?? "");
  const month = period ? Number(period[2]) : 0;
  return period && month >= 1 && month <= 12 ? { year: Number(period[1]), month } : fallback;
}

/** (2028, 1) → "2028-01", the value of `?mes=`. */
export function monthParam(year: number, month: number): string {
  return `${year}-${String(month).padStart(2, "0")}`;
}

/** The local calendar month right now. */
export function currentMonth(now: Date = new Date()): YearMonth {
  return { year: now.getFullYear(), month: now.getMonth() + 1 };
}
