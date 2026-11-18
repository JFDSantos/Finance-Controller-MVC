const BRL = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export const MONTH_NAMES = [
  "janeiro",
  "fevereiro",
  "março",
  "abril",
  "maio",
  "junho",
  "julho",
  "agosto",
  "setembro",
  "outubro",
  "novembro",
  "dezembro",
] as const;

/** "R$ 1.234,50" (Intl puts a no-break space after "R$"). */
export function formatBRL(value: number): string {
  return BRL.format(value);
}

/** "2028-01-31" → "31/01/2028". Reads the three parts; never goes through Date, so no timezone can move the day. */
export function formatDate(iso: string): string {
  const [year, month, day] = iso.slice(0, 10).split("-");
  return `${day}/${month}/${year}`;
}

/** (2028, 1) → "janeiro de 2028". `month` is 1-12. */
export function monthLabel(year: number, month: number): string {
  return `${MONTH_NAMES[month - 1]} de ${year}`;
}

/** Moves a (year, month) pair by `delta` months. */
export function shiftMonth(year: number, month: number, delta: number): { year: number; month: number } {
  const index = year * 12 + (month - 1) + delta;
  return { year: Math.floor(index / 12), month: (index % 12) + 1 };
}

const pad = (n: number) => String(n).padStart(2, "0");

/** Today's local date as "yyyy-MM-dd". */
export function todayIso(now: Date = new Date()): string {
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

/** "yyyy-MM-dd" for a given local day. */
export function isoDate(year: number, month: number, day: number): string {
  return `${year}-${pad(month)}-${pad(day)}`;
}

const GROUPED = /^\d{1,3}(\.\d{3})+$/;
const DIGITS = /^\d+$/;

/**
 * Parses an amount typed the Brazilian way: "1.234,50", "1234,5", "12,3", "1.234" (thousands).
 * A lone dot followed by one or two digits ("12.5") is read as a decimal point, as phone keypads
 * in other locales send it. Returns null when the text is not a number, is zero or negative,
 * or has more than two decimals.
 */
export function parseAmount(input: string): number | null {
  const text = input.replace(/R\$/i, "").replace(/[\s\u00a0]/g, "");
  if (!text) return null;

  let whole: string;
  let fraction = "";
  const commaParts = text.split(",");
  if (commaParts.length > 2) return null;
  if (commaParts.length === 2) {
    [whole, fraction] = commaParts;
    if (!/^\d{1,2}$/.test(fraction)) return null;
  } else {
    const dotDecimal = /^(\d+)\.(\d{1,2})$/.exec(text);
    if (dotDecimal) {
      [, whole, fraction] = dotDecimal;
    } else {
      whole = text;
    }
  }

  if (GROUPED.test(whole)) whole = whole.replace(/\./g, "");
  else if (!DIGITS.test(whole)) return null;

  const cents = Number(whole) * 100 + Number(fraction.padEnd(2, "0"));
  if (!Number.isSafeInteger(cents) || cents <= 0) return null;
  return cents / 100;
}

/** A stored value written back the way a person types it: 1234.5 → "1234,50". */
export function amountInput(value: number): string {
  return value.toFixed(2).replace(".", ",");
}

/** Sums money without floating-point drift (works in cents). */
export function sumMoney(values: number[]): number {
  return values.reduce((total, value) => total + Math.round(value * 100), 0) / 100;
}
