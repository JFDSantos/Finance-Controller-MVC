import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import type { CategoryTotal, MonthPoint } from "../api/types";
import { CategoryChart } from "./CategoryChart";
import { MonthlyChart } from "./MonthlyChart";

const plain = (text: string | null) => (text ?? "").replace(/\u00a0/g, " ").trim();

const rows: CategoryTotal[] = [
  { categoryId: 1, categoryName: "Moradia", type: "Expense", total: 1800 },
  { categoryId: 2, categoryName: "Salário", type: "Income", total: 5000 },
  { categoryId: 3, categoryName: "Mercado", type: "Expense", total: 550.25 },
  { categoryId: 4, categoryName: "Lazer", type: "Expense", total: 0.75 },
];

const points: MonthPoint[] = Array.from({ length: 12 }, (_, i) => {
  const index = 2026 * 12 + 10 - (11 - i);
  return {
    year: Math.floor(index / 12),
    month: (index % 12) + 1,
    income: i === 11 ? 5000.5 : i * 100,
    expense: i === 11 ? 2350.25 : i * 50,
  };
});

describe("chart table views", () => {
  it("chart table view lists the same numbers as the chart (by category)", async () => {
    const user = userEvent.setup();
    render(<CategoryChart rows={rows} period="janeiro de 2028" />);

    const bars = within(screen.getByRole("list", { name: /Despesas por categoria/ })).getAllByRole("listitem");
    const chartPairs = bars.map((bar) => [
      plain(bar.querySelector(".category-bar__name")!.textContent),
      plain(bar.querySelector(".category-bar__value")!.textContent),
    ]);
    expect(chartPairs).toEqual([
      ["Moradia", "R$ 1.800,00"],
      ["Mercado", "R$ 550,25"],
      ["Lazer", "R$ 0,75"],
    ]);

    await user.click(screen.getByRole("button", { name: "Ver como tabela" }));
    const table = screen.getByRole("table", { name: /Despesas por categoria/ });
    const tableRows = within(table).getAllByRole("row").slice(1);
    expect(tableRows.map((r) => within(r).getAllByRole("cell").slice(0, 2).map((c) => plain(c.textContent)))).toEqual(
      chartPairs,
    );
    expect(plain(tableRows[0].textContent)).toContain("77%");
    expect(screen.getByRole("button", { name: "Ver como tabela" })).toHaveAttribute("aria-pressed", "true");
    expect(screen.queryByRole("list", { name: /Despesas por categoria/ })).not.toBeInTheDocument();
  });

  it("chart table view lists the same numbers as the chart (month by month)", async () => {
    const user = userEvent.setup();
    render(<MonthlyChart points={points} selected={{ year: 2026, month: 11 }} />);

    // The readout carries the focused month (the selected one) as direct labels.
    const readout = screen.getByTestId("monthly-readout");
    expect(plain(readout.textContent)).toContain("novembro de 2026");
    expect(plain(readout.textContent)).toContain("R$ 5.000,50");
    expect(plain(readout.textContent)).toContain("R$ 2.350,25");

    // Keyboard: one tab stop, arrows walk the months.
    const plot = screen.getByRole("group", { name: /Receitas e despesas dos últimos 12 meses/ });
    plot.focus();
    await user.keyboard("{ArrowLeft}");
    expect(plain(readout.textContent)).toContain("outubro de 2026");
    expect(plain(readout.textContent)).toContain("R$ 1.000,00");
    expect(plain(readout.textContent)).toContain("R$ 500,00");

    await user.click(screen.getByRole("button", { name: "Ver como tabela" }));
    const table = screen.getByRole("table", { name: /Receitas e despesas/ });
    const tableRows = within(table).getAllByRole("row").slice(1);
    expect(tableRows).toHaveLength(12);
    const last = within(tableRows[11]).getAllByRole("cell").map((c) => plain(c.textContent));
    expect(last[0]).toBe("nov/26");
    expect(last[1]).toBe("R$ 5.000,50");
    expect(last[2]).toBe("R$ 2.350,25");
    expect(last[3]).toBe("+R$ 2.650,25");
    const october = within(tableRows[10]).getAllByRole("cell").map((c) => plain(c.textContent));
    expect(october.slice(0, 3)).toEqual(["out/26", "R$ 1.000,00", "R$ 500,00"]);
  });

  it("shows a calm note when the category chart has no expenses", () => {
    render(<CategoryChart rows={[rows[1]]} period="janeiro de 2028" />);
    expect(screen.getByText("Nenhuma despesa em janeiro de 2028.")).toBeInTheDocument();
  });

  it("keeps a frame and says so when all twelve months are zero", () => {
    const zeros = points.map((p) => ({ ...p, income: 0, expense: 0 }));
    render(<MonthlyChart points={zeros} selected={{ year: 2026, month: 11 }} />);
    expect(screen.getByText("Nenhuma movimentação nos últimos 12 meses.")).toBeInTheDocument();
    expect(screen.getByRole("group", { name: /Receitas e despesas dos últimos 12 meses/ })).toBeInTheDocument();
  });
});
