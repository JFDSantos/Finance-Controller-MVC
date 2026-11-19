import { useId, useState } from "react";
import type { ReactNode } from "react";
import { Icon } from "./Icon";

export type ChartView = "chart" | "table";

interface ChartPanelProps {
  title: string;
  description: string;
  /** Offer the table view (every chart has one; off only when there is nothing to show). */
  hasTable: boolean;
  children(view: ChartView): ReactNode;
  className?: string;
}

/** A white panel holding one chart, its title and the switch to its table twin. */
export function ChartPanel({ title, description, hasTable, children, className }: ChartPanelProps) {
  const titleId = useId();
  const [view, setView] = useState<ChartView>("chart");
  const shown = hasTable ? view : "chart";
  return (
    <section className={["chart-panel", className ?? ""].join(" ").trim()} aria-labelledby={titleId}>
      <header className="chart-panel__head">
        <div>
          <h3 className="chart-panel__title" id={titleId}>
            {title}
          </h3>
          <p className="chart-panel__desc">{description}</p>
        </div>
        {hasTable ? (
          <button
            type="button"
            className="chart-panel__toggle"
            aria-pressed={view === "table"}
            onClick={() => setView((v) => (v === "table" ? "chart" : "table"))}
          >
            <Icon name="table" size={18} />
            Ver como tabela
          </button>
        ) : null}
      </header>
      {children(shown)}
    </section>
  );
}
