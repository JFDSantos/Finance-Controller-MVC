import type { CategoryTotal } from "../api/types";
import { MAX_SLICES, OTHERS_LABEL, share, toCategorySlices, toCents } from "../lib/chartData";
import { formatBRL } from "../lib/format";
import { formatShare } from "../lib/money";
import { ChartPanel } from "./ChartPanel";

interface CategoryChartProps {
  rows: CategoryTotal[];
  period: string;
}

/**
 * Where the month's expenses went: ranked horizontal bars, one hue (one series; the names are direct labels),
 * each row carrying its name, amount and share. The table twin lists the same rows.
 */
export function CategoryChart({ rows, period }: CategoryChartProps) {
  const slices = toCategorySlices(rows);
  const expenseRows = rows.filter((r) => r.type === "Expense" && toCents(r.total) > 0);
  const totalCents = expenseRows.reduce((sum, r) => sum + toCents(r.total), 0);
  const total = totalCents / 100;
  const max = slices.reduce((m, s) => Math.max(m, s.value), 0);
  const folded = expenseRows.length > MAX_SLICES ? expenseRows.length - (MAX_SLICES - 1) : 0;
  const name = `Despesas por categoria em ${period}`;

  return (
    <ChartPanel
      className="category-chart"
      title="Para onde foi o dinheiro"
      description={
        slices.length > 0
          ? `${formatBRL(total)} em despesas, da maior para a menor.`
          : "Despesas do mês, por categoria."
      }
      hasTable={slices.length > 0}
    >
      {(view) =>
        slices.length === 0 ? (
          <p className="chart-panel__empty">Nenhuma despesa em {period}.</p>
        ) : view === "table" ? (
          <div className="chart-table-wrap">
            <table className="chart-table">
              <caption className="visually-hidden">{name}</caption>
              <thead>
                <tr>
                  <th scope="col">Categoria</th>
                  <th scope="col" className="chart-table__num">
                    Valor
                  </th>
                  <th scope="col" className="chart-table__num">
                    % das despesas
                  </th>
                </tr>
              </thead>
              <tbody>
                {slices.map((slice, i) => (
                  <tr key={`${i}-${slice.name}`}>
                    <td>
                      {slice.name}
                      {isFolded(slice.name, i, folded) ? (
                        <span className="chart-table__note"> ({folded} categorias)</span>
                      ) : null}
                    </td>
                    <td className="chart-table__num">{formatBRL(slice.value)}</td>
                    <td className="chart-table__num">{formatShare(share(slice.value, total))}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <ol className="category-bars" aria-label={name}>
            {slices.map((slice, i) => {
              const width = max > 0 ? (slice.value / max) * 100 : 0;
              return (
                <li key={`${i}-${slice.name}`} className="category-bar">
                  <span className="category-bar__label">
                    <span className="category-bar__name">{slice.name}</span>
                    {isFolded(slice.name, i, folded) ? (
                      <span className="category-bar__note">{folded} categorias</span>
                    ) : null}
                  </span>
                  <span className="category-bar__figures">
                    <span className="category-bar__value">{formatBRL(slice.value)}</span>
                    <span className="category-bar__share">{formatShare(share(slice.value, total))}</span>
                  </span>
                  <span className="category-bar__track" aria-hidden="true">
                    <span className="category-bar__fill" style={{ width: `max(2px, ${width}%)` }} />
                  </span>
                </li>
              );
            })}
          </ol>
        )
      }
    </ChartPanel>
  );
}

/** The folded slice is always the last one, and only when folding happened. */
function isFolded(name: string, index: number, folded: number): boolean {
  return folded > 0 && index === MAX_SLICES - 1 && name === OTHERS_LABEL;
}
