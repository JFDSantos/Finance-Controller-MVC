import { formatBRL } from "./format";

const PERCENT = new Intl.NumberFormat("pt-BR", { style: "percent", maximumFractionDigits: 0 });

/** 0.7712 → "77%"; a non-zero share under half a percent reads "<1%", never "0%". */
export function formatShare(ratio: number): string {
  if (ratio > 0 && ratio < 0.005) return "<1%";
  return PERCENT.format(ratio);
}

/** Sign as text (never only colour): "+R$ 10,00", "−R$ 10,00", "R$ 0,00". */
export function signedBRL(value: number): string {
  const cents = Math.round(value * 100);
  if (cents === 0) return formatBRL(0);
  return `${cents > 0 ? "+" : "−"}${formatBRL(Math.abs(cents) / 100)}`;
}

const COMPACT = new Intl.NumberFormat("pt-BR", { notation: "compact", maximumFractionDigits: 1 });

/** Axis ticks: "R$ 0", "R$ 2,5 mil", "R$ 1 mi". */
export function compactBRL(value: number): string {
  return `R$ ${COMPACT.format(value)}`;
}
