import { api } from "./client";
import type { MonthlySummary, MonthPoint } from "./types";

export const summaryPath = (year: number, month: number) => `/api/Summary?year=${year}&month=${month}`;
export const seriesPath = (months: number) => `/api/Summary/series?months=${months}`;

export function getMonthlySummary(year: number, month: number): Promise<MonthlySummary> {
  return api.get<MonthlySummary>(summaryPath(year, month));
}

/** The last `months` months, chronological, ending at the current month (UTC). */
export function getSeries(months = 12): Promise<MonthPoint[]> {
  return api.get<MonthPoint[]>(seriesPath(months));
}
