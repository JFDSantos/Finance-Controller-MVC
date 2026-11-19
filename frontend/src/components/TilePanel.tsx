import type { CSSProperties } from "react";

/**
 * A panel of square azulejo modules (after Athos Bulcão's Brasília panels):
 * three figures, four rotations, placed by a seeded sequence so the panel is
 * the same on every render. Decorative only.
 */

export type Figure = "quarter" | "half" | "diagonal";

const FIGURES: Record<Figure, string> = {
  quarter: "M0 0H100A100 100 0 0 1 0 100Z",
  half: "M0 100A50 50 0 0 1 100 100Z",
  diagonal: "M0 0H100L0 100Z",
};
const FIGURE_ORDER: Figure[] = ["quarter", "half", "quarter", "diagonal"];

function mulberry32(seed: number) {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = a;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

interface TilePanelProps {
  cols: number;
  rows: number;
  seed?: number;
  /** Quarter-turn entrance, staggered across the panel. */
  animate?: boolean;
  className?: string;
  /** Indexes of tiles left out of the panel (only grout shows). */
  missing?: number[];
}

const TILE = 100;
const GROUT = 6;

export function TilePanel({ cols, rows, seed = 1958, animate = false, className, missing = [] }: TilePanelProps) {
  const random = mulberry32(seed);
  const step = TILE + GROUT;
  const width = cols * step - GROUT;
  const height = rows * step - GROUT;
  const tiles = Array.from({ length: cols * rows }, (_, i) => {
    const figure = FIGURE_ORDER[Math.floor(random() * FIGURE_ORDER.length)];
    const rotation = Math.floor(random() * 4) * 90;
    const accent = random() < 0.16;
    return { i, x: (i % cols) * step, y: Math.floor(i / cols) * step, figure, rotation, accent };
  });

  return (
    <svg
      className={["tile-panel", animate ? "tile-panel--animate" : "", className ?? ""].join(" ").trim()}
      viewBox={`0 0 ${width} ${height}`}
      aria-hidden="true"
      focusable="false"
    >
      {tiles.filter((t) => !missing.includes(t.i)).map((t) => (
        <g key={t.i} transform={`translate(${t.x} ${t.y})`}>
          <rect className="tile-panel__ground" width={TILE} height={TILE} />
          <g transform={`rotate(${t.rotation} 50 50)`}>
            <path
              className={t.accent ? "tile-panel__figure tile-panel__figure--accent" : "tile-panel__figure"}
              d={FIGURES[t.figure]}
              style={{ "--i": t.i } as CSSProperties}
            />
          </g>
        </g>
      ))}
    </svg>
  );
}

interface TileGlyphProps {
  figure: Figure;
  /** Quarter turns, clockwise. A half-disc at 0 sits on the bottom edge (rising); at 180 it hangs from the top. */
  rotation?: 0 | 90 | 180 | 270;
  className?: string;
}

/** One module of the tile panel, used as a glyph beside data (summary tiles). Decorative: the text carries the meaning. */
export function TileGlyph({ figure, rotation = 0, className }: TileGlyphProps) {
  return (
    <svg
      className={["tile-glyph", className ?? ""].join(" ").trim()}
      viewBox="0 0 100 100"
      aria-hidden="true"
      focusable="false"
    >
      <rect className="tile-glyph__ground" width={TILE} height={TILE} />
      <path className="tile-glyph__figure" d={FIGURES[figure]} transform={`rotate(${rotation} 50 50)`} />
    </svg>
  );
}
