import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { SummaryCards } from "./SummaryCards";

const plain = (text: string | null) => (text ?? "").replace(/\u00a0/g, " ");

const summary = (income: number, expense: number) => ({
  year: 2028,
  month: 1,
  income,
  expense,
  balance: Math.round((income - expense) * 100) / 100,
  byCategory: [],
});

describe("SummaryCards", () => {
  it("SummaryCards shows balance sign without relying on color", () => {
    const { rerender } = render(<SummaryCards summary={summary(1000, 1150.5)} period="janeiro de 2028" />);
    const balance = screen.getByRole("group", { name: /Saldo/ });
    // The minus sign and the word are text, visible to everyone, not only a red colour.
    expect(plain(balance.textContent)).toContain("−R$ 150,50");
    expect(balance).toHaveTextContent(/negativo/i);

    rerender(<SummaryCards summary={summary(1000, 400)} period="janeiro de 2028" />);
    const positive = screen.getByRole("group", { name: /Saldo/ });
    expect(plain(positive.textContent)).toContain("+R$ 600,00");
    expect(positive).toHaveTextContent(/positivo/i);

    rerender(<SummaryCards summary={summary(500, 500)} period="janeiro de 2028" />);
    const zero = screen.getByRole("group", { name: /Saldo/ });
    expect(plain(zero.textContent)).toContain("R$ 0,00");
    expect(zero).toHaveTextContent(/zerado/i);
  });

  it("shows income and expense with signs and the share of income spent", () => {
    render(<SummaryCards summary={summary(1000, 720)} period="janeiro de 2028" />);
    expect(plain(screen.getByRole("group", { name: /Receitas/ }).textContent)).toContain("+R$ 1.000,00");
    const expense = screen.getByRole("group", { name: /Despesas/ });
    expect(plain(expense.textContent)).toContain("−R$ 720,00");
    expect(expense).toHaveTextContent("72% das receitas");
  });

  it("says when there was no income to compare with", () => {
    render(<SummaryCards summary={summary(0, 80)} period="janeiro de 2028" />);
    expect(screen.getByRole("group", { name: /Despesas/ })).toHaveTextContent("Sem receitas no mês");
  });
});
