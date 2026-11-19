import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { mockFetch } from "../test/fetchMock";
import { emptySummary, januarySummary, zeroSeries } from "../test/summaryFixtures";
import Dashboard from "./Dashboard";

const plain = (text: string | null) => (text ?? "").replace(/ /g, " ");

function Location() {
  const location = useLocation();
  return <output data-testid="location">{location.pathname + location.search}</output>;
}

function renderPage(path = "/?mes=2028-01") {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route
          path="/"
          element={
            <>
              <Dashboard />
              <Location />
            </>
          }
        />
      </Routes>
    </MemoryRouter>,
  );
}

const urls = (fetchMock: ReturnType<typeof mockFetch>) => fetchMock.mock.calls.map(([url]) => String(url));

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

describe("Dashboard", () => {
  it("Dashboard requests summary and series for the selected month", async () => {
    const fetchMock = mockFetch({
      "GET /api/Summary?year=2028&month=1": { status: 200, body: januarySummary },
      "GET /api/Summary/series?months=12": { status: 200, body: zeroSeries(2026, 11) },
    });
    renderPage();

    const balance = await screen.findByRole("group", { name: /Saldo/ });
    expect(plain(balance.textContent)).toContain("+R$ 2.650,25");
    expect(urls(fetchMock)).toEqual(
      expect.arrayContaining(["/api/Summary?year=2028&month=1", "/api/Summary/series?months=12"]),
    );
    expect(screen.getByRole("heading", { level: 1, name: "Painel" })).toBeInTheDocument();
    expect(await screen.findByRole("list", { name: /Despesas por categoria/ })).toBeInTheDocument();
  });

  it("defaults to the current month", async () => {
    vi.useFakeTimers({ toFake: ["Date"] });
    vi.setSystemTime(new Date(2026, 10, 18, 12));
    const fetchMock = mockFetch({
      "GET /api/Summary?year=2026&month=11": { status: 200, body: emptySummary(2026, 11) },
      "GET /api/Summary/series?months=12": { status: 200, body: zeroSeries(2026, 11) },
    });
    renderPage("/");
    expect(await screen.findByRole("heading", { name: "Nenhuma movimentação em novembro de 2026" })).toBeInTheDocument();
    expect(urls(fetchMock)).toContain("/api/Summary?year=2026&month=11");
  });

  it("Dashboard shows empty state for a month without data", async () => {
    mockFetch({
      "GET /api/Summary?year=2028&month=1": { status: 200, body: emptySummary(2028, 1) },
      "GET /api/Summary/series?months=12": { status: 200, body: zeroSeries(2026, 11) },
    });
    renderPage();

    expect(await screen.findByRole("heading", { name: "Nenhuma movimentação em janeiro de 2028" })).toBeInTheDocument();
    const link = screen.getByRole("link", { name: "Ir para Movimentações" });
    expect(link).toHaveAttribute("href", "/movimentacoes?mes=2028-01");
    expect(screen.queryByRole("group", { name: /Saldo/ })).not.toBeInTheDocument();
  });

  it("Dashboard shows error with retry", async () => {
    const fetchMock = mockFetch({
      "GET /api/Summary?year=2028&month=1": [
        { status: 500, body: { code: "INTERNAL_ERROR", message: "boom" } },
        { status: 200, body: januarySummary },
      ],
      "GET /api/Summary/series?months=12": { status: 200, body: zeroSeries(2026, 11) },
    });
    const user = userEvent.setup();
    renderPage();

    expect(await screen.findByText("Tivemos um problema no servidor. Tente novamente em instantes.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Tentar novamente" }));
    expect(await screen.findByRole("group", { name: /Saldo/ })).toBeInTheDocument();
    expect(urls(fetchMock).filter((u) => u === "/api/Summary?year=2028&month=1")).toHaveLength(2);
  });

  it("shows the series error in its own panel with retry", async () => {
    mockFetch({
      "GET /api/Summary?year=2028&month=1": { status: 200, body: januarySummary },
      "GET /api/Summary/series?months=12": [
        { status: 503, body: {} },
        { status: 200, body: zeroSeries(2026, 11) },
      ],
    });
    const user = userEvent.setup();
    renderPage();

    expect(await screen.findByText("Algo deu errado. Tente novamente.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Tentar novamente" }));
    expect(await screen.findByText("Nenhuma movimentação nos últimos 12 meses.")).toBeInTheDocument();
  });

  it("changing month updates the URL and refetches the summary", async () => {
    const fetchMock = mockFetch({
      "GET /api/Summary?year=2028&month=1": { status: 200, body: januarySummary },
      "GET /api/Summary?year=2027&month=12": { status: 200, body: emptySummary(2027, 12) },
      "GET /api/Summary/series?months=12": { status: 200, body: zeroSeries(2026, 11) },
    });
    const user = userEvent.setup();
    renderPage();

    await screen.findByRole("group", { name: /Saldo/ });
    await user.click(screen.getByRole("button", { name: "Mês anterior" }));

    await waitFor(() => expect(screen.getByTestId("location")).toHaveTextContent("/?mes=2027-12"));
    expect(await screen.findByRole("heading", { name: "Nenhuma movimentação em dezembro de 2027" })).toBeInTheDocument();
    expect(urls(fetchMock)).toContain("/api/Summary?year=2027&month=12");
    // The 12-month series does not depend on the chosen month: fetched once.
    expect(urls(fetchMock).filter((u) => u.startsWith("/api/Summary/series"))).toHaveLength(1);
  });
});
