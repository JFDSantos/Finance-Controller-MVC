import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { ApiError } from "../api/client";
import { getMonthlySummary, getSeries } from "../api/summary";
import type { MonthlySummary, MonthPoint } from "../api/types";
import { CategoryChart } from "../components/CategoryChart";
import { MonthFilter } from "../components/MonthFilter";
import { MonthlyChart } from "../components/MonthlyChart";
import { Notice } from "../components/Notice";
import { SummaryCards } from "../components/SummaryCards";
import { TilePanel } from "../components/TilePanel";
import { errorMessage } from "../i18n/errors";
import { toCents } from "../lib/chartData";
import { monthLabel } from "../lib/format";
import { currentMonth, monthParam, readMonthParam } from "../lib/monthParam";
import { usePageTitle } from "./usePageTitle";

type Load<T> = { status: "loading" } | { status: "error"; message: string } | { status: "ready"; data: T; refreshing: boolean };

const SERIES_MONTHS = 12;

function messageOf(error: unknown): string {
  return errorMessage(error instanceof ApiError ? error.code : "UNKNOWN");
}

function isEmptyMonth(summary: MonthlySummary): boolean {
  return toCents(summary.income) === 0 && toCents(summary.expense) === 0;
}

export default function Dashboard() {
  usePageTitle("Painel");
  const [params, setParams] = useSearchParams();
  const current = useMemo(() => currentMonth(), []);
  // The month lives in the address (?mes=2028-01), as on the movements page, so reload and back keep it.
  const { year, month } = readMonthParam(params, current);
  const period = monthLabel(year, month);

  const [summary, setSummary] = useState<Load<MonthlySummary>>({ status: "loading" });
  const [series, setSeries] = useState<Load<MonthPoint[]>>({ status: "loading" });
  const [summaryReload, setSummaryReload] = useState(0);
  const [seriesReload, setSeriesReload] = useState(0);

  useEffect(() => {
    let active = true;
    // Keep the previous month on screen, dimmed, while the next one loads.
    setSummary((previous) => (previous.status === "ready" ? { ...previous, refreshing: true } : { status: "loading" }));
    getMonthlySummary(year, month)
      .then((data) => active && setSummary({ status: "ready", data, refreshing: false }))
      .catch((error) => active && setSummary({ status: "error", message: messageOf(error) }));
    return () => {
      active = false;
    };
  }, [year, month, summaryReload]);

  useEffect(() => {
    let active = true;
    setSeries({ status: "loading" });
    getSeries(SERIES_MONTHS)
      .then((data) => active && setSeries({ status: "ready", data, refreshing: false }))
      .catch((error) => active && setSeries({ status: "error", message: messageOf(error) }));
    return () => {
      active = false;
    };
  }, [seriesReload]);

  function changeMonth(nextYear: number, nextMonth: number) {
    setParams(new URLSearchParams({ mes: monthParam(nextYear, nextMonth) }), { replace: true });
  }

  const ready = summary.status === "ready" ? summary : undefined;
  const empty = ready ? isEmptyMonth(ready.data) : false;

  return (
    <section className="page dashboard" aria-labelledby="page-title">
      <header className="page__header">
        <div>
          <h1 className="page__title" id="page-title">
            Painel
          </h1>
          <p className="page__lead">Como o mês está indo: saldo, receitas, despesas e para onde foi o dinheiro.</p>
        </div>
      </header>

      <div className="filters dashboard__filters">
        <MonthFilter year={year} month={month} current={current} onChange={changeMonth} />
      </div>

      <section className="dashboard__month" aria-labelledby="period-title" aria-busy={ready?.refreshing || undefined}>
        <h2 className="dashboard__period" id="period-title">
          {period}
        </h2>

        {summary.status === "error" ? (
          <ErrorState message={summary.message} onRetry={() => setSummaryReload((n) => n + 1)} />
        ) : summary.status === "loading" ? (
          <SummarySkeleton />
        ) : empty ? (
          <div className="state state--empty dashboard__empty">
            <TilePanel cols={7} rows={1} seed={year * 12 + month} className="state__tiles" />
            <h3 className="state__title">Nenhuma movimentação em {period}</h3>
            <p className="state__text">
              Quando você registrar receitas e despesas deste mês, o saldo e o gráfico por categoria aparecem aqui.
            </p>
            <Link className="button button--primary" to={`/movimentacoes?mes=${monthParam(year, month)}`}>
              Ir para Movimentações
            </Link>
          </div>
        ) : ready ? (
          <div className={ready.refreshing ? "dashboard__summary dashboard__summary--refreshing" : "dashboard__summary"}>
            <SummaryCards summary={ready.data} period={period} />
          </div>
        ) : null}
      </section>

      <div className={empty || !ready ? "dashboard__charts dashboard__charts--single" : "dashboard__charts"}>
        {ready && !empty ? (
          <div className={ready.refreshing ? "dashboard__refreshing" : undefined}>
            <CategoryChart rows={ready.data.byCategory} period={period} />
          </div>
        ) : null}
        {series.status === "ready" ? (
          <MonthlyChart points={series.data} selected={{ year, month }} />
        ) : series.status === "error" ? (
          <section className="chart-panel" aria-labelledby="series-error-title">
            <h3 className="chart-panel__title" id="series-error-title">
              Últimos 12 meses
            </h3>
            <ErrorState message={series.message} onRetry={() => setSeriesReload((n) => n + 1)} bare />
          </section>
        ) : (
          <ChartSkeleton />
        )}
      </div>
    </section>
  );
}

function ErrorState({ message, onRetry, bare = false }: { message: string; onRetry(): void; bare?: boolean }) {
  return (
    <div className={bare ? "state--error chart-panel__error" : "state state--error"} role="alert">
      <Notice tone="danger">{message}</Notice>
      <button type="button" className="button button--secondary" onClick={onRetry}>
        Tentar novamente
      </button>
    </div>
  );
}

function SummarySkeleton() {
  return (
    <div className="summary-skeleton" aria-busy="true">
      <span className="visually-hidden">Carregando o resumo do mês…</span>
      {[0, 1, 2].map((tile) => (
        <div key={tile} className="summary-skeleton__tile" aria-hidden="true">
          <span className="skeleton__bar skeleton__bar--short" />
          <span className="skeleton__bar" />
        </div>
      ))}
    </div>
  );
}

function ChartSkeleton() {
  return (
    <div className="chart-panel chart-skeleton" aria-busy="true">
      <span className="visually-hidden">Carregando os últimos 12 meses…</span>
      <div className="chart-skeleton__bars" aria-hidden="true">
        {Array.from({ length: 12 }, (_, i) => (
          <span key={i} className="chart-skeleton__bar" style={{ height: `${30 + ((i * 37) % 60)}%` }} />
        ))}
      </div>
    </div>
  );
}
