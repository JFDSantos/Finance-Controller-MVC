import type { MonthPoint, MonthlySummary } from "../api/types";

/** Twelve zero-filled months ending at (year, month), as the series endpoint answers. */
export function zeroSeries(year: number, month: number, months = 12): MonthPoint[] {
  return Array.from({ length: months }, (_, i) => {
    const index = year * 12 + (month - 1) - (months - 1 - i);
    return { year: Math.floor(index / 12), month: (index % 12) + 1, income: 0, expense: 0 };
  });
}

export const januarySummary: MonthlySummary = {
  year: 2028,
  month: 1,
  income: 5000.5,
  expense: 2350.25,
  balance: 2650.25,
  byCategory: [
    { categoryId: 2, categoryName: "Salário", type: "Income", total: 5000.5 },
    { categoryId: 1, categoryName: "Moradia", type: "Expense", total: 1800 },
    { categoryId: 3, categoryName: "Mercado", type: "Expense", total: 550.25 },
  ],
};

export const emptySummary = (year: number, month: number): MonthlySummary => ({
  year,
  month,
  income: 0,
  expense: 0,
  balance: 0,
  byCategory: [],
});
