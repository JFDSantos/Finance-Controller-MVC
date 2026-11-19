import { describe, expect, it } from "vitest";
import { currentMonth, monthParam, readMonthParam } from "./monthParam";

const fallback = { year: 2026, month: 11 };

describe("month query parameter", () => {
  it("reads a valid ?mes= and falls back otherwise", () => {
    expect(readMonthParam(new URLSearchParams("mes=2028-01"), fallback)).toEqual({ year: 2028, month: 1 });
    expect(readMonthParam(new URLSearchParams("mes=2028-13"), fallback)).toEqual(fallback);
    expect(readMonthParam(new URLSearchParams("mes=jan"), fallback)).toEqual(fallback);
    expect(readMonthParam(new URLSearchParams(""), fallback)).toEqual(fallback);
  });

  it("writes the zero-padded value and reads the local month", () => {
    expect(monthParam(2028, 1)).toBe("2028-01");
    expect(currentMonth(new Date(2026, 0, 31, 23, 59))).toEqual({ year: 2026, month: 1 });
  });
});
