import { describe, expect, it } from "vitest";
import { amountInput, formatBRL, formatDate, monthLabel, parseAmount, shiftMonth } from "./format";

// Intl separates "R$" from the figure with a no-break space; compare with a plain one.
const plain = (text: string) => text.replace(/\u00a0/g, " ");

describe("format", () => {
  it("formatBRL formats thousands and cents", () => {
    expect(plain(formatBRL(1234.5))).toBe("R$ 1.234,50");
    expect(plain(formatBRL(0.1 + 0.2))).toBe("R$ 0,30");
    expect(plain(formatBRL(1000000))).toBe("R$ 1.000.000,00");
    expect(plain(formatBRL(7))).toBe("R$ 7,00");
  });

  it("formatDate formats ISO date without timezone shift", () => {
    expect(formatDate("2028-01-31")).toBe("31/01/2028");
    expect(formatDate("2028-02-29")).toBe("29/02/2028");
    expect(formatDate("2026-01-01")).toBe("01/01/2026");
  });

  it("monthLabel formats Portuguese month", () => {
    expect(monthLabel(2028, 1)).toBe("janeiro de 2028");
    expect(monthLabel(2026, 3)).toBe("março de 2026");
    expect(monthLabel(2026, 12)).toBe("dezembro de 2026");
  });

  it("shiftMonth crosses year boundaries", () => {
    expect(shiftMonth(2028, 1, -1)).toEqual({ year: 2027, month: 12 });
    expect(shiftMonth(2027, 12, 1)).toEqual({ year: 2028, month: 1 });
    expect(shiftMonth(2026, 6, 1)).toEqual({ year: 2026, month: 7 });
  });

  it("parseAmount accepts Brazilian formats and rejects invalid", () => {
    expect(parseAmount("1.234,50")).toBe(1234.5);
    expect(parseAmount("1234,5")).toBe(1234.5);
    expect(parseAmount("12,3")).toBe(12.3);
    expect(parseAmount("12")).toBe(12);
    expect(parseAmount(" R$ 1.000.000,01 ")).toBe(1000000.01);
    expect(parseAmount("0,01")).toBe(0.01);
    expect(parseAmount("12.5")).toBe(12.5);
    expect(parseAmount("1.234")).toBe(1234);

    expect(parseAmount("1.000")).toBe(1000);
    expect(parseAmount("999.999")).toBe(999999);
    expect(parseAmount("0.001")).toBeNull();
    expect(parseAmount("01.000")).toBeNull();
    expect(parseAmount("0.000")).toBeNull();
    expect(parseAmount("")).toBeNull();
    expect(parseAmount("0")).toBeNull();
    expect(parseAmount("0,00")).toBeNull();
    expect(parseAmount("-5")).toBeNull();
    expect(parseAmount("-5,00")).toBeNull();
    expect(parseAmount("1,234")).toBeNull();
    expect(parseAmount("12,")).toBeNull();
    expect(parseAmount("1.23.4")).toBeNull();
    expect(parseAmount("12,3,4")).toBeNull();
    expect(parseAmount("abc")).toBeNull();
    expect(parseAmount("1e3")).toBeNull();
  });

  it("amountInput writes a stored value back in Brazilian input form", () => {
    expect(amountInput(1234.5)).toBe("1234,50");
    expect(amountInput(12)).toBe("12,00");
  });
});
