import { useLayoutEffect, useRef, useState } from "react";
import type { KeyboardEvent } from "react";
import type { MonthPoint } from "../api/types";
import { niceTicks, subtractMoney, toMonthlySeries } from "../lib/chartData";
import { formatBRL, monthLabel } from "../lib/format";
import type { YearMonth } from "../lib/monthParam";
import { compactBRL, signedBRL } from "../lib/money";
import { ChartPanel } from "./ChartPanel";

interface MonthlyChartProps {
  /** Chronological months from `GET /api/Summary/series`. */
  points: MonthPoint[];
  /** The month chosen on the page; highlighted when it falls inside the series. */
  selected: YearMonth;
}

const HEIGHT = 208; // plot area
const TOP = 12; // room above the tallest tick label
const AXIS = 28; // x-axis label band, inside the SVG so the container never clips it
const LEFT = 64; // y tick labels
const RIGHT = 4;
const MAX_BAR = 20;
const GAP = 2; // surface gap between the income and expense bar of one month
const RADIUS = 4;

/** Measures an element's width; jsdom and first paint fall back to `fallback`. */
function useWidth<T extends HTMLElement>(fallback: number) {
  const ref = useRef<T>(null);
  const [width, setWidth] = useState(fallback);
  useLayoutEffect(() => {
    const element = ref.current;
    if (!element) return;
    const measure = () => setWidth(element.clientWidth || fallback);
    measure();
    if (typeof ResizeObserver === "undefined") return;
    const observer = new ResizeObserver(measure);
    observer.observe(element);
    return () => observer.disconnect();
  }, [fallback]);
  return [ref, width] as const;
}

/** A column with a 4px rounded data end and a square foot on the baseline. */
function columnPath(x: number, width: number, baseline: number, height: number): string {
  const r = Math.min(RADIUS, width / 2, height);
  const top = baseline - height;
  return [
    `M${x} ${baseline}`,
    `V${top + r}`,
    `Q${x} ${top} ${x + r} ${top}`,
    `H${x + width - r}`,
    `Q${x + width} ${top} ${x + width} ${top + r}`,
    `V${baseline}`,
    "Z",
  ].join("");
}

/**
 * Income against expense, month by month, as paired columns on one axis. The readout above the plot is the legend
 * and the direct label of the month in focus (the selected one, or the one under the pointer or keyboard).
 */
export function MonthlyChart({ points, selected }: MonthlyChartProps) {
  const series = toMonthlySeries(points);
  const selectedIndex = points.findIndex((p) => p.year === selected.year && p.month === selected.month);
  const restingIndex = selectedIndex >= 0 ? selectedIndex : points.length - 1;
  const [pointed, setPointed] = useState<number | null>(null);
  const [focused, setFocused] = useState(false);
  const [plotRef, width] = useWidth<HTMLDivElement>(640);

  const active = Math.min(pointed ?? restingIndex, points.length - 1);
  const allZero = series.every((p) => p.income === 0 && p.expense === 0);
  const ticks = niceTicks(Math.max(0, ...series.map((p) => Math.max(p.income, p.expense))));
  const top = ticks[ticks.length - 1];
  const plotWidth = Math.max(0, width - LEFT - RIGHT);
  const band = series.length > 0 ? plotWidth / series.length : 0;
  const pair = Math.min(band * 0.64, MAX_BAR * 2 + GAP);
  const bar = Math.max(1, (pair - GAP) / 2);
  const baseline = TOP + HEIGHT;
  const y = (value: number) => baseline - (value / top) * HEIGHT;
  const labelEvery = width < 480 ? 3 : width < 720 ? 2 : 1;

  function onKeyDown(event: KeyboardEvent) {
    const last = points.length - 1;
    const moves: Record<string, number> = {
      ArrowLeft: Math.max(0, active - 1),
      ArrowRight: Math.min(last, active + 1),
      Home: 0,
      End: last,
    };
    if (event.key in moves) {
      event.preventDefault();
      setPointed(moves[event.key]);
    }
  }

  const point = points[active];
  const values = series[active];

  return (
    <ChartPanel
      className="monthly-chart"
      title="Últimos 12 meses"
      description="Receitas e despesas de cada mês, lado a lado."
      hasTable={series.length > 0}
    >
      {(view) =>
        view === "table" ? (
          <div className="chart-table-wrap">
            <table className="chart-table">
              <caption className="visually-hidden">Receitas e despesas, mês a mês, nos últimos 12 meses</caption>
              <thead>
                <tr>
                  <th scope="col">Mês</th>
                  <th scope="col" className="chart-table__num">
                    Receitas
                  </th>
                  <th scope="col" className="chart-table__num">
                    Despesas
                  </th>
                  <th scope="col" className="chart-table__num">
                    Saldo
                  </th>
                </tr>
              </thead>
              <tbody>
                {series.map((p, i) => (
                  <tr key={p.label} className={i === selectedIndex ? "chart-table__row--selected" : undefined}>
                    <td>
                      <abbr title={monthLabel(points[i].year, points[i].month)}>{p.label}</abbr>
                    </td>
                    <td className="chart-table__num">{formatBRL(p.income)}</td>
                    <td className="chart-table__num">{formatBRL(p.expense)}</td>
                    <td className="chart-table__num">{signedBRL(subtractMoney(p.income, p.expense))}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <>
            {point && values ? (
              <div className="monthly-readout" data-testid="monthly-readout" aria-live="polite">
                <p className="monthly-readout__month">{monthLabel(point.year, point.month)}</p>
                <dl className="monthly-readout__list">
                  <div className="monthly-readout__item">
                    <dt>
                      <span className="series-key series-key--income" aria-hidden="true" />
                      Receitas
                    </dt>
                    <dd>{formatBRL(values.income)}</dd>
                  </div>
                  <div className="monthly-readout__item">
                    <dt>
                      <span className="series-key series-key--expense" aria-hidden="true" />
                      Despesas
                    </dt>
                    <dd>{formatBRL(values.expense)}</dd>
                  </div>
                  <div className="monthly-readout__item monthly-readout__item--balance">
                    <dt>Saldo</dt>
                    <dd>{signedBRL(subtractMoney(values.income, values.expense))}</dd>
                  </div>
                </dl>
              </div>
            ) : null}
            <div
              ref={plotRef}
              className="monthly-chart__plot"
              role="group"
              tabIndex={0}
              aria-label="Receitas e despesas dos últimos 12 meses. Use as setas para percorrer os meses."
              onKeyDown={onKeyDown}
              onFocus={() => setFocused(true)}
              onBlur={() => setFocused(false)}
              onPointerLeave={() => setPointed(null)}
            >
              <svg width={width} height={baseline + AXIS} aria-hidden="true" focusable="false">
                {ticks.map((tick) => (
                  <g key={tick}>
                    <line
                      className={tick === 0 ? "monthly-chart__baseline" : "monthly-chart__grid"}
                      x1={LEFT}
                      x2={LEFT + plotWidth}
                      y1={y(tick)}
                      y2={y(tick)}
                    />
                    <text className="monthly-chart__tick" x={LEFT - 8} y={y(tick)} dy="0.32em" textAnchor="end">
                      {compactBRL(tick)}
                    </text>
                  </g>
                ))}
                {series.map((p, i) => {
                  const cx = LEFT + band * (i + 0.5);
                  const x0 = cx - pair / 2;
                  const incomeHeight = baseline - y(p.income);
                  const expenseHeight = baseline - y(p.expense);
                  // Every nth month counted back from the latest; the selected month always, and its neighbours give way to it.
                  const labelled =
                    i === selectedIndex ||
                    ((series.length - 1 - i) % labelEvery === 0 &&
                      (selectedIndex < 0 || Math.abs(i - selectedIndex) >= labelEvery));
                  return (
                    <g key={p.label}>
                      {i === active ? (
                        <rect
                          className={focused ? "monthly-chart__band monthly-chart__band--focus" : "monthly-chart__band"}
                          x={LEFT + band * i + 1}
                          y={TOP - 6}
                          width={Math.max(0, band - 2)}
                          height={HEIGHT + 6}
                          rx={RADIUS}
                        />
                      ) : null}
                      {incomeHeight > 0 ? (
                        <path className="monthly-chart__bar--income" d={columnPath(x0, bar, baseline, Math.max(1, incomeHeight))} />
                      ) : null}
                      {expenseHeight > 0 ? (
                        <path
                          className="monthly-chart__bar--expense"
                          d={columnPath(x0 + bar + GAP, bar, baseline, Math.max(1, expenseHeight))}
                        />
                      ) : null}
                      {labelled ? (
                        <text
                          className={
                            i === selectedIndex ? "monthly-chart__label monthly-chart__label--selected" : "monthly-chart__label"
                          }
                          x={cx}
                          y={baseline + 18}
                          textAnchor="middle"
                        >
                          {p.label}
                        </text>
                      ) : null}
                      {/* The hit target is the whole month band, much larger than the bars. */}
                      <rect
                        className="monthly-chart__hit"
                        x={LEFT + band * i}
                        y={0}
                        width={band}
                        height={baseline + AXIS}
                        onPointerEnter={() => setPointed(i)}
                        onPointerDown={() => setPointed(i)}
                      />
                    </g>
                  );
                })}
              </svg>
              {allZero ? <p className="monthly-chart__empty">Nenhuma movimentação nos últimos 12 meses.</p> : null}
            </div>
          </>
        )
      }
    </ChartPanel>
  );
}
