import { useId } from "react";
import type { ReactNode } from "react";
import type { MonthlySummary } from "../api/types";
import { formatBRL } from "../lib/format";
import { share, toCents } from "../lib/chartData";
import { formatShare } from "../lib/money";
import { TileGlyph } from "./TilePanel";

interface SummaryCardsProps {
  summary: MonthlySummary;
  /** "janeiro de 2028", for the accessible name of the group. */
  period: string;
}

const METER_TILES = 10;

/**
 * The month's balance, income and expense, set as a small azulejo panel: tiles separated by cobalt grout.
 * Sign and direction are text and shape (a half-disc rising or hanging), never colour alone.
 */
export function SummaryCards({ summary, period }: SummaryCardsProps) {
  const balanceCents = toCents(summary.balance);
  const tone = balanceCents > 0 ? "positive" : balanceCents < 0 ? "negative" : "zero";
  const spent = share(summary.expense, summary.income);
  const hasIncome = toCents(summary.income) > 0;

  return (
    <div className="summary" role="group" aria-label={`Resumo de ${period}`}>
      <Tile
        className={`summary-tile--balance summary-tile--${tone}`}
        label="Saldo"
        glyph={<TileGlyph figure={tone === "zero" ? "diagonal" : "half"} rotation={tone === "negative" ? 180 : 0} />}
        value={
          <>
            {tone !== "zero" ? <span aria-hidden="true">{tone === "positive" ? "+" : "−"}</span> : null}
            {formatBRL(Math.abs(balanceCents) / 100)}
          </>
        }
        note={
          tone === "positive" ? (
            <span>
              <strong>Positivo</strong>: as receitas cobriram as despesas.
            </span>
          ) : tone === "negative" ? (
            <span>
              <strong>Negativo</strong>: as despesas passaram as receitas.
            </span>
          ) : (
            <span>
              <strong>Zerado</strong>: receitas e despesas empataram.
            </span>
          )
        }
      />
      <Tile
        className="summary-tile--income"
        label="Receitas"
        glyph={<TileGlyph figure="half" />}
        value={
          <>
            {toCents(summary.income) > 0 ? <span aria-hidden="true">+</span> : null}
            {formatBRL(summary.income)}
          </>
        }
        note="Entrou no mês"
      />
      <Tile
        className="summary-tile--expense"
        label="Despesas"
        glyph={<TileGlyph figure="half" rotation={180} />}
        value={
          <>
            {toCents(summary.expense) > 0 ? <span aria-hidden="true">−</span> : null}
            {formatBRL(summary.expense)}
          </>
        }
        note={
          hasIncome ? (
            <>
              <SpentMeter ratio={spent} />
              <span>
                {formatShare(spent)} das receitas
              </span>
            </>
          ) : (
            "Sem receitas no mês"
          )
        }
      />
    </div>
  );
}

function Tile({
  className,
  label,
  glyph,
  value,
  note,
}: {
  className: string;
  label: string;
  glyph: ReactNode;
  value: ReactNode;
  note: ReactNode;
}) {
  const labelId = useId();
  return (
    <div className={`summary-tile ${className}`} role="group" aria-labelledby={labelId}>
      <div className="summary-tile__head">
        <p className="summary-tile__label" id={labelId}>
          {label}
        </p>
        {glyph}
      </div>
      <p className="summary-tile__value">{value}</p>
      <div className="summary-tile__note">{note}</div>
    </div>
  );
}

/** Ten small tiles, each a tenth of the month's income, filled by what was spent. Decorative: the percentage is text. */
function SpentMeter({ ratio }: { ratio: number }) {
  const filled = Math.min(ratio, 1) * METER_TILES;
  return (
    <svg className="spent-meter" viewBox={`0 0 ${METER_TILES * 12 - 2} 10`} aria-hidden="true" focusable="false">
      {Array.from({ length: METER_TILES }, (_, i) => {
        const fill = Math.max(0, Math.min(1, filled - i));
        return (
          <g key={i} transform={`translate(${i * 12} 0)`}>
            <rect className="spent-meter__track" width={10} height={10} />
            {fill > 0 ? <rect className="spent-meter__fill" width={10 * fill} height={10} /> : null}
          </g>
        );
      })}
    </svg>
  );
}
