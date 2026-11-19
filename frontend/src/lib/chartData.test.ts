import { describe, expect, it } from "vitest";
import type { CategoryTotal, MonthPoint } from "../api/types";
import {
  MAX_SLICES,
  OTHERS_LABEL,
  niceTicks,
  share,
  shortMonthLabel,
  subtractMoney,
  toCategorySlices,
  toMonthlySeries,
} from "./chartData";

const row = (categoryId: number, categoryName: string, total: number, type: CategoryTotal["type"] = "Expense") => ({
  categoryId,
  categoryName,
  type,
  total,
});

describe("toCategorySlices", () => {
  it("toCategorySlices ignores income rows and sorts desc", () => {
    const slices = toCategorySlices([
      row(1, "Mercado", 412.3),
      row(2, "Salário", 5000, "Income"),
      row(3, "Moradia", 1800),
      row(4, "Lazer", 90.15),
    ]);
    expect(slices).toEqual([
      { name: "Moradia", value: 1800 },
      { name: "Mercado", value: 412.3 },
      { name: "Lazer", value: 90.15 },
    ]);
  });

  it("toCategorySlices handles empty input", () => {
    expect(toCategorySlices([])).toEqual([]);
    expect(toCategorySlices([row(2, "Salário", 5000, "Income")])).toEqual([]);
  });

  it("breaks ties by name so the order is stable", () => {
    const slices = toCategorySlices([row(1, "Transporte", 100), row(2, "Educação", 100), row(3, "Água", 100)]);
    expect(slices.map((s) => s.name)).toEqual(["Água", "Educação", "Transporte"]);
  });

  it("drops zero rows", () => {
    expect(toCategorySlices([row(1, "Mercado", 0), row(2, "Lazer", 10)])).toEqual([{ name: "Lazer", value: 10 }]);
  });

  it(`folds the tail beyond ${MAX_SLICES} slices into "${OTHERS_LABEL}", summed in cents, placed last`, () => {
    const rows = [
      row(1, "A", 900),
      row(2, "B", 800),
      row(3, "C", 700),
      row(4, "D", 600),
      row(5, "E", 500),
      row(6, "F", 400),
      row(7, "G", 300),
      row(8, "H", 0.1),
      row(9, "I", 0.2),
      row(10, "J", 250),
    ];
    const slices = toCategorySlices(rows);
    expect(slices).toHaveLength(MAX_SLICES);
    expect(slices.slice(0, MAX_SLICES - 1).map((s) => s.name)).toEqual(["A", "B", "C", "D", "E", "F", "G"]);
    // 250 + 0.2 + 0.1 in floats is 250.30000000000001; cents keep it exact.
    expect(slices[MAX_SLICES - 1]).toEqual({ name: OTHERS_LABEL, value: 250.3 });
  });

  it(`keeps exactly ${MAX_SLICES} categories without folding`, () => {
    const rows = Array.from({ length: MAX_SLICES }, (_, i) => row(i + 1, `Cat ${i + 1}`, 100 - i));
    const slices = toCategorySlices(rows);
    expect(slices).toHaveLength(MAX_SLICES);
    expect(slices.some((s) => s.name === OTHERS_LABEL)).toBe(false);
  });
});

const point = (year: number, month: number, income: number, expense: number): MonthPoint => ({
  year,
  month,
  income,
  expense,
});

describe("toMonthlySeries", () => {
  it("toMonthlySeries keeps chronological order and pt-BR month labels", () => {
    const series = toMonthlySeries([
      point(2025, 11, 100, 50),
      point(2025, 12, 200.1, 0.2),
      point(2026, 1, 300, 400),
      point(2026, 2, 0, 10),
    ]);
    expect(series).toEqual([
      { label: "nov/25", income: 100, expense: 50 },
      { label: "dez/25", income: 200.1, expense: 0.2 },
      { label: "jan/26", income: 300, expense: 400 },
      { label: "fev/26", income: 0, expense: 10 },
    ]);
  });

  it("toMonthlySeries handles all-zero months", () => {
    const points = Array.from({ length: 12 }, (_, i) => point(2026, i + 1, 0, 0));
    const series = toMonthlySeries(points);
    expect(series).toHaveLength(12);
    expect(series.every((p) => p.income === 0 && p.expense === 0)).toBe(true);
    expect(series.map((p) => p.label)).toEqual([
      "jan/26",
      "fev/26",
      "mar/26",
      "abr/26",
      "mai/26",
      "jun/26",
      "jul/26",
      "ago/26",
      "set/26",
      "out/26",
      "nov/26",
      "dez/26",
    ]);
  });

  it("toMonthlySeries handles an empty list", () => {
    expect(toMonthlySeries([])).toEqual([]);
  });

  it("formats years with two digits", () => {
    expect(shortMonthLabel(2005, 3)).toBe("mar/05");
    expect(shortMonthLabel(2100, 12)).toBe("dez/00");
  });
});

describe("chart helpers", () => {
  it("share works in cents and is 0 for an empty whole", () => {
    expect(share(0.1 + 0.2, 0.6)).toBe(0.5);
    expect(share(10, 0)).toBe(0);
  });

  it("subtractMoney has no float drift", () => {
    expect(subtractMoney(0.3, 0.1)).toBe(0.2);
  });

  it("niceTicks starts at zero, covers the max and keeps a frame for zero data", () => {
    expect(niceTicks(4200)).toEqual([0, 2000, 4000, 6000]);
    expect(niceTicks(9000)).toEqual([0, 2500, 5000, 7500, 10000]);
    expect(niceTicks(0)).toEqual([0, 25, 50, 75, 100]);
  });
});
