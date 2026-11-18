import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import type { Movement } from "../api/types";
import { mockFetch } from "../test/fetchMock";
import Movements from "./Movements";

const categories = [
  { id: 1, name: "Moradia" },
  { id: 2, name: "Salário" },
];

const rent: Movement = {
  id: 41,
  type: "Expense",
  description: "Aluguel",
  movimentDate: "2028-01-05",
  isRecurring: true,
  categoryId: 1,
  categoryName: "Moradia",
  value: 1200,
};

const salary: Movement = {
  id: 12,
  type: "Income",
  description: "Salário de janeiro",
  movimentDate: "2028-01-31",
  isRecurring: false,
  categoryId: 2,
  categoryName: "Salário",
  value: 5000.5,
};

const plain = (text: string | null) => (text ?? "").replace(/\u00a0/g, " ");

function renderPage(path = "/movimentacoes?mes=2028-01") {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/movimentacoes" element={<Movements />} />
      </Routes>
    </MemoryRouter>,
  );
}

function getUrls(fetchMock: ReturnType<typeof mockFetch>) {
  return fetchMock.mock.calls.filter(([, init]) => (init?.method ?? "GET") === "GET").map(([url]) => String(url));
}

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

describe("Movements page", () => {
  it("Movements page lists, filters by category and month through query params", async () => {
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2028&month=1": { status: 200, body: [salary, rent] },
      "GET /api/Movement?year=2028&month=1&categoryId=1": { status: 200, body: [rent] },
      "GET /api/Movement?year=2027&month=12&categoryId=1": { status: 200, body: [] },
      "GET /api/Movement?year=2027&month=12&categoryId=1&type=Expense": { status: 200, body: [] },
    });
    const user = userEvent.setup();
    renderPage();

    const list = await screen.findByRole("table", { name: /janeiro de 2028/ });
    const rows = within(list).getAllByRole("row").slice(1);
    expect(rows).toHaveLength(2);
    expect(plain(rows[0].textContent)).toContain("31/01/2028");
    expect(plain(rows[0].textContent)).toContain("+R$ 5.000,50");
    expect(plain(rows[1].textContent)).toContain("−R$ 1.200,00");
    expect(within(rows[1]).getByText("Recorrente")).toBeInTheDocument();
    const totals = screen.getByRole("group", { name: "Totais do período" });
    expect(plain(totals.textContent)).toContain("R$ 3.800,50");

    await user.selectOptions(screen.getByRole("combobox", { name: "Categoria" }), "1");
    await waitFor(() => expect(screen.getAllByRole("row")).toHaveLength(2));
    expect(getUrls(fetchMock)).toContain("/api/Movement?year=2028&month=1&categoryId=1");

    await user.click(screen.getByRole("button", { name: "Mês anterior" }));
    expect(await screen.findByText(/Nada encontrado com esses filtros/)).toBeInTheDocument();
    expect(getUrls(fetchMock)).toContain("/api/Movement?year=2027&month=12&categoryId=1");
    expect(screen.getByRole("combobox", { name: "Mês" })).toHaveValue("12");
    expect(screen.getByRole("combobox", { name: "Ano" })).toHaveValue("2027");

    await user.click(screen.getByRole("radio", { name: "Despesas" }));
    await waitFor(() =>
      expect(getUrls(fetchMock)).toContain("/api/Movement?year=2027&month=12&categoryId=1&type=Expense"),
    );
  });

  it("defaults to the current month", async () => {
    vi.useFakeTimers({ toFake: ["Date"] });
    vi.setSystemTime(new Date(2026, 10, 18, 12));
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2026&month=11": { status: 200, body: [] },
    });
    renderPage("/movimentacoes");

    expect(await screen.findByRole("heading", { name: "novembro de 2026" })).toBeInTheDocument();
    expect(getUrls(fetchMock)).toContain("/api/Movement?year=2026&month=11");
  });

  it("Movements page shows empty state", async () => {
    mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2028&month=1": { status: 200, body: [] },
    });
    const user = userEvent.setup();
    renderPage();

    expect(await screen.findByRole("heading", { name: "Nenhuma movimentação em janeiro de 2028" })).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Registrar movimentação" }));
    expect(await screen.findByRole("dialog", { name: "Nova movimentação" })).toBeInTheDocument();
    expect(screen.getByLabelText("Data")).toHaveValue("2028-01-01");
  });

  it("shows an API error with a retry", async () => {
    mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2028&month=1": [
        { status: 500, body: { code: "INTERNAL_ERROR" } },
        { status: 200, body: [rent] },
      ],
    });
    const user = userEvent.setup();
    renderPage();

    expect(await screen.findByText(/problema no servidor/)).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Tentar novamente" }));
    expect(await screen.findByText("Aluguel")).toBeInTheDocument();
  });

  it("creates a movement and reloads the month it belongs to", async () => {
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2028&month=1": { status: 200, body: [rent] },
      "GET /api/Movement?year=2028&month=2": { status: 200, body: [] },
      "POST /api/Expense": { status: 201, body: { id: 50 } },
    });
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Aluguel");

    await user.click(screen.getByRole("button", { name: "Nova movimentação" }));
    const dialog = await screen.findByRole("dialog", { name: "Nova movimentação" });
    await user.type(within(dialog).getByLabelText("Descrição"), "Luz");
    const date = within(dialog).getByLabelText("Data");
    await user.clear(date);
    await user.type(date, "2028-02-10");
    await user.selectOptions(within(dialog).getByLabelText("Categoria"), "1");
    await user.type(within(dialog).getByLabelText("Valor"), "180,90");
    await user.click(within(dialog).getByRole("button", { name: "Salvar" }));

    await waitFor(() => expect(getUrls(fetchMock)).toContain("/api/Movement?year=2028&month=2"));
    expect(await screen.findByRole("status")).toHaveTextContent("Despesa salva em fevereiro de 2028.");
  });

  it("deletes a movement after confirmation", async () => {
    const fetchMock = mockFetch({
      "GET /api/Category": { status: 200, body: categories },
      "GET /api/Movement?year=2028&month=1": [
        { status: 200, body: [rent] },
        { status: 200, body: [] },
      ],
      "DELETE /api/Expense/41": { status: 204 },
    });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Excluir Aluguel" }));
    const dialog = await screen.findByRole("dialog", { name: "Excluir esta movimentação?" });
    expect(plain(dialog.textContent)).toContain("R$ 1.200,00");
    await user.click(within(dialog).getByRole("button", { name: "Excluir" }));

    expect(await screen.findByRole("heading", { name: "Nenhuma movimentação em janeiro de 2028" })).toBeInTheDocument();
    expect(fetchMock.mock.calls.some(([url, init]) => url === "/api/Expense/41" && init?.method === "DELETE")).toBe(true);
  });
});
