import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { ApiError } from "../api/client";
import { categoriesApi } from "../api/categories";
import { movementListPath, movementsApi } from "../api/movements";
import type { Category, Movement, MovementFilter, MovementType } from "../api/types";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { Dialog } from "../components/Dialog";
import { Icon } from "../components/Icon";
import { MonthFilter } from "../components/MonthFilter";
import { MovementForm } from "../components/MovementForm";
import type { SavedMovement } from "../components/MovementForm";
import { Notice } from "../components/Notice";
import { Segmented } from "../components/Segmented";
import { SelectField } from "../components/SelectField";
import { TilePanel } from "../components/TilePanel";
import { errorMessage } from "../i18n/errors";
import { formatBRL, formatDate, isoDate, monthLabel, sumMoney, todayIso } from "../lib/format";
import { usePageTitle } from "./usePageTitle";

type TypeFilter = "all" | "income" | "expense";

const TYPE_FILTERS = [
  { value: "all" as const, label: "Todas" },
  { value: "income" as const, label: "Receitas" },
  { value: "expense" as const, label: "Despesas" },
];

const TYPE_OF: Record<Exclude<TypeFilter, "all">, MovementType> = { income: "Income", expense: "Expense" };
const URL_TYPE: Record<string, Exclude<TypeFilter, "all">> = { receitas: "income", despesas: "expense" };
const TYPE_URL: Record<Exclude<TypeFilter, "all">, string> = { income: "receitas", expense: "despesas" };

interface PageFilter {
  year: number;
  month: number;
  categoryId?: number;
  type: TypeFilter;
}

/** Page filters live in the address (?mes=2028-01&categoria=3&tipo=despesas) so reload and back keep them. */
function readFilter(params: URLSearchParams, current: { year: number; month: number }): PageFilter {
  const period = /^(\d{4})-(\d{2})$/.exec(params.get("mes") ?? "");
  const month = period ? Number(period[2]) : 0;
  const valid = period && month >= 1 && month <= 12;
  const category = Number(params.get("categoria"));
  return {
    year: valid ? Number(period[1]) : current.year,
    month: valid ? month : current.month,
    categoryId: Number.isInteger(category) && category > 0 ? category : undefined,
    type: URL_TYPE[params.get("tipo") ?? ""] ?? "all",
  };
}

function writeFilter(filter: PageFilter): URLSearchParams {
  const params = new URLSearchParams();
  params.set("mes", `${filter.year}-${String(filter.month).padStart(2, "0")}`);
  if (filter.categoryId !== undefined) params.set("categoria", String(filter.categoryId));
  if (filter.type !== "all") params.set("tipo", TYPE_URL[filter.type]);
  return params;
}

function describe(movement: Movement): string {
  return movement.description?.trim() || `movimentação de ${formatDate(movement.movimentDate)}`;
}

function Money({ value, sign }: { value: number; sign?: "+" | "−" }) {
  return (
    <>
      {sign ? <span aria-hidden="true">{sign}</span> : null}
      {formatBRL(value)}
    </>
  );
}

type Load<T> = { status: "loading" } | { status: "error"; message: string } | { status: "ready"; data: T; refreshing: boolean };

export default function Movements() {
  usePageTitle("Movimentações");
  const [params, setParams] = useSearchParams();
  const current = useMemo(() => {
    const now = new Date();
    return { year: now.getFullYear(), month: now.getMonth() + 1 };
  }, []);
  const filter = readFilter(params, current);
  const apiFilter: MovementFilter = {
    year: filter.year,
    month: filter.month,
    categoryId: filter.categoryId,
    type: filter.type === "all" ? undefined : TYPE_OF[filter.type],
  };
  const listPath = movementListPath(apiFilter);

  const [categories, setCategories] = useState<Load<Category[]>>({ status: "loading" });
  const [movements, setMovements] = useState<Load<Movement[]>>({ status: "loading" });
  const [categoriesReload, setCategoriesReload] = useState(0);
  const [movementsReload, setMovementsReload] = useState(0);
  const [announcement, setAnnouncement] = useState<string>();
  const [formTarget, setFormTarget] = useState<Movement | "new" | null>(null);
  const [deleting, setDeleting] = useState<Movement | null>(null);
  const headingRef = useRef<HTMLHeadingElement>(null);

  useEffect(() => {
    let active = true;
    categoriesApi
      .list()
      .then((data) => active && setCategories({ status: "ready", data, refreshing: false }))
      .catch((error) => active && setCategories({ status: "error", message: messageOf(error) }));
    return () => {
      active = false;
    };
  }, [categoriesReload]);

  useEffect(() => {
    let active = true;
    // Keep the rows on screen while a new filter loads; show the skeleton only when there is nothing yet.
    setMovements((previous) =>
      previous.status === "ready" ? { ...previous, refreshing: true } : { status: "loading" },
    );
    movementsApi
      .list(apiFilterFromPath(listPath))
      .then((data) => active && setMovements({ status: "ready", data, refreshing: false }))
      .catch((error) => active && setMovements({ status: "error", message: messageOf(error) }));
    return () => {
      active = false;
    };
  }, [listPath, movementsReload]);

  const changeFilter = useCallback(
    (next: PageFilter, keepAnnouncement = false) => {
      if (!keepAnnouncement) setAnnouncement(undefined);
      setParams(writeFilter(next), { replace: true });
    },
    [setParams],
  );

  function retry() {
    if (categories.status === "error") setCategoriesReload((n) => n + 1);
    if (movements.status === "error") setMovementsReload((n) => n + 1);
  }

  function handleSaved(saved: SavedMovement, edited: boolean) {
    const [year, month] = saved.movimentDate.split("-").map(Number);
    const moved = year !== filter.year || month !== filter.month;
    // Show the month the movement landed in, and drop any filter that would hide it.
    const next: PageFilter = {
      year,
      month,
      categoryId: filter.categoryId === saved.categoryId ? filter.categoryId : undefined,
      type: filter.type !== "all" && TYPE_OF[filter.type] === saved.type ? filter.type : "all",
    };
    const noun = saved.type === "Income" ? "Receita" : "Despesa";
    setAnnouncement(`${noun} ${edited ? "atualizada" : "salva"}${moved ? ` em ${monthLabel(year, month)}` : ""}.`);
    setFormTarget(null);
    changeFilter(next, true);
    setMovementsReload((n) => n + 1);
  }

  async function confirmDelete() {
    if (!deleting) return;
    await movementsApi.remove(deleting.type, deleting.id);
    setDeleting(null);
    setAnnouncement(`${deleting.type === "Income" ? "Receita" : "Despesa"} excluída.`);
    setMovementsReload((n) => n + 1);
  }

  const categoryList = categories.status === "ready" ? categories.data : [];
  const isCurrentMonth = filter.year === current.year && filter.month === current.month;
  const defaultDate = isCurrentMonth ? todayIso() : isoDate(filter.year, filter.month, 1);
  const period = monthLabel(filter.year, filter.month);
  const hasFilters = filter.categoryId !== undefined || filter.type !== "all";
  const activeCategory = categoryList.find((c) => c.id === filter.categoryId);
  const loadError =
    categories.status === "error" ? categories.message : movements.status === "error" ? movements.message : undefined;

  return (
    <section className="page movements" aria-labelledby="page-title">
      <header className="page__header">
        <div>
          <h1 className="page__title" id="page-title">
            Movimentações
          </h1>
          <p className="page__lead">Receitas e despesas, mês a mês.</p>
        </div>
        <button
          type="button"
          className="button button--primary page__action"
          onClick={() => setFormTarget("new")}
          disabled={categories.status !== "ready"}
        >
          <Icon name="plus" />
          Nova movimentação
        </button>
      </header>

      <div className="filters">
        <MonthFilter
          year={filter.year}
          month={filter.month}
          current={current}
          onChange={(year, month) => changeFilter({ ...filter, year, month })}
        />
        <SelectField
          label="Categoria"
          className="filters__category"
          value={filter.categoryId === undefined ? "" : String(filter.categoryId)}
          onChange={(event) =>
            changeFilter({ ...filter, categoryId: event.target.value ? Number(event.target.value) : undefined })
          }
        >
          <option value="">Todas as categorias</option>
          {categoryList.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </SelectField>
        <Segmented
          legend="Tipo"
          options={TYPE_FILTERS}
          value={filter.type}
          onChange={(type) => changeFilter({ ...filter, type })}
          className="filters__type"
        />
      </div>

      <div role="status" className="live-region">
        {announcement ? <Notice>{announcement}</Notice> : null}
      </div>

      <section className="ledger" aria-labelledby="ledger-title">
        <div className="ledger__head">
          <div>
            <h2 className="ledger__title" id="ledger-title" ref={headingRef} tabIndex={-1}>
              {period}
            </h2>
            {hasFilters ? (
              <p className="ledger__filters">
                Mostrando{" "}
                {[
                  activeCategory?.name ?? (filter.categoryId !== undefined ? "uma categoria" : undefined),
                  filter.type === "income" ? "só receitas" : filter.type === "expense" ? "só despesas" : undefined,
                ]
                  .filter(Boolean)
                  .join(", ")}
                .{" "}
                <button
                  type="button"
                  className="link-button"
                  onClick={() => changeFilter({ ...filter, categoryId: undefined, type: "all" })}
                >
                  Limpar filtros
                </button>
              </p>
            ) : null}
          </div>
          {movements.status === "ready" && movements.data.length > 0 ? (
            <Totals items={movements.data} type={filter.type} />
          ) : null}
        </div>

        {loadError ? (
          <div className="state state--error">
            <Notice tone="danger">{loadError}</Notice>
            <button type="button" className="button button--secondary" onClick={retry}>
              Tentar novamente
            </button>
          </div>
        ) : movements.status === "loading" ? (
          <LedgerSkeleton />
        ) : movements.status === "ready" && movements.data.length === 0 ? (
          hasFilters ? (
            <div className="state">
              <h3 className="state__title">Nada encontrado com esses filtros</h3>
              <p className="state__text">
                Não há movimentações em {period} para os filtros escolhidos.
              </p>
              <button
                type="button"
                className="button button--secondary"
                onClick={() => changeFilter({ ...filter, categoryId: undefined, type: "all" })}
              >
                Limpar filtros
              </button>
            </div>
          ) : (
            <div className="state state--empty">
              <TilePanel cols={7} rows={1} seed={filter.year * 12 + filter.month} className="state__tiles" />
              <h3 className="state__title">Nenhuma movimentação em {period}</h3>
              <p className="state__text">Registre uma receita ou despesa para acompanhar como o mês está indo.</p>
              <button
                type="button"
                className="button button--primary"
                onClick={() => setFormTarget("new")}
                disabled={categories.status !== "ready"}
              >
                <Icon name="plus" />
                Registrar movimentação
              </button>
            </div>
          )
        ) : movements.status === "ready" ? (
          <LedgerTable
            items={movements.data}
            caption={`Movimentações de ${period}`}
            busy={movements.refreshing}
            onEdit={setFormTarget}
            onDelete={setDeleting}
          />
        ) : null}
      </section>

      <Dialog
        open={formTarget !== null}
        title={formTarget === "new" ? "Nova movimentação" : "Editar movimentação"}
        onClose={() => setFormTarget(null)}
        fallbackFocus={() => headingRef.current}
        wide
      >
        {formTarget !== null ? (
          <MovementForm
            key={formTarget === "new" ? "new" : formTarget.id}
            categories={categoryList}
            movement={formTarget === "new" ? undefined : formTarget}
            defaultDate={defaultDate}
            onCancel={() => setFormTarget(null)}
            onSaved={(saved) => handleSaved(saved, formTarget !== "new")}
          />
        ) : null}
      </Dialog>

      <ConfirmDialog
        open={deleting !== null}
        title="Excluir esta movimentação?"
        confirmLabel="Excluir"
        pendingLabel="Excluindo…"
        onConfirm={confirmDelete}
        onCancel={() => setDeleting(null)}
        fallbackFocus={() => headingRef.current}
      >
        {deleting ? (
          <p>
            <strong>{describe(deleting)}</strong>, {deleting.type === "Income" ? "receita" : "despesa"} de{" "}
            <strong className="nowrap">{formatBRL(deleting.value)}</strong> em {formatDate(deleting.movimentDate)}, será
            apagada. Não é possível desfazer.
          </p>
        ) : null}
      </ConfirmDialog>
    </section>
  );
}

function messageOf(error: unknown): string {
  return errorMessage(error instanceof ApiError ? error.code : "UNKNOWN");
}

/** The effect depends on the path string only; rebuild the filter from it so the dependency list stays honest. */
function apiFilterFromPath(path: string): MovementFilter {
  const query = new URLSearchParams(path.split("?")[1]);
  const read = (key: string) => (query.has(key) ? Number(query.get(key)) : undefined);
  return {
    year: Number(query.get("year")),
    month: read("month"),
    categoryId: read("categoryId"),
    type: (query.get("type") as MovementType | null) ?? undefined,
  };
}

function Totals({ items, type }: { items: Movement[]; type: TypeFilter }) {
  const income = sumMoney(items.filter((m) => m.type === "Income").map((m) => m.value));
  const expense = sumMoney(items.filter((m) => m.type === "Expense").map((m) => m.value));
  const balance = sumMoney([income, -expense]);
  const count = `${items.length} ${items.length === 1 ? "movimentação" : "movimentações"}`;

  return (
    <div className="totals" role="group" aria-label="Totais do período">
      <p className="totals__count">{count}</p>
      <dl className="totals__list">
        {type !== "expense" ? (
          <div className="totals__item totals__item--income">
            <dt>Receitas</dt>
            <dd>
              <Money value={income} sign="+" />
            </dd>
          </div>
        ) : null}
        {type !== "income" ? (
          <div className="totals__item totals__item--expense">
            <dt>Despesas</dt>
            <dd>
              <Money value={expense} sign="−" />
            </dd>
          </div>
        ) : null}
        {type === "all" ? (
          <div className="totals__item totals__item--balance">
            <dt>Saldo</dt>
            <dd>
              <Money value={Math.abs(balance)} sign={balance < 0 ? "−" : undefined} />
              {balance < 0 ? <span className="visually-hidden"> negativo</span> : null}
            </dd>
          </div>
        ) : null}
      </dl>
    </div>
  );
}

function LedgerTable({
  items,
  caption,
  busy,
  onEdit,
  onDelete,
}: {
  items: Movement[];
  caption: string;
  busy: boolean;
  onEdit(movement: Movement): void;
  onDelete(movement: Movement): void;
}) {
  // Explicit roles keep table semantics where the phone layout turns rows into stacked cards.
  return (
    <table className="ledger__table" role="table" aria-busy={busy || undefined}>
      <caption className="visually-hidden">{caption}</caption>
      <thead className="ledger__thead" role="rowgroup">
        <tr role="row">
          <th scope="col" role="columnheader">
            Data
          </th>
          <th scope="col" role="columnheader">
            Descrição
          </th>
          <th scope="col" role="columnheader">
            Categoria
          </th>
          <th scope="col" role="columnheader" className="ledger__num">
            Valor
          </th>
          <th scope="col" role="columnheader">
            <span className="visually-hidden">Ações</span>
          </th>
        </tr>
      </thead>
      <tbody role="rowgroup">
        {items.map((movement) => {
          const income = movement.type === "Income";
          const label = describe(movement);
          return (
            <tr key={`${movement.type}-${movement.id}`} role="row" className="ledger__row">
              <td role="cell" className="ledger__date">
                {formatDate(movement.movimentDate)}
              </td>
              <td role="cell" className="ledger__desc">
                <span className={movement.description?.trim() ? undefined : "ledger__muted"}>
                  {movement.description?.trim() || "Sem descrição"}
                </span>
                {movement.isRecurring ? (
                  <span className="tag">
                    <Icon name="repeat" size={14} />
                    Recorrente
                  </span>
                ) : null}
              </td>
              <td role="cell" className="ledger__cat">
                {movement.categoryName}
              </td>
              <td role="cell" className={`ledger__num ledger__value ledger__value--${income ? "income" : "expense"}`}>
                <span className="visually-hidden">{income ? "Receita: " : "Despesa: "}</span>
                <Money value={movement.value} sign={income ? "+" : "−"} />
              </td>
              <td role="cell" className="ledger__actions">
                <button type="button" className="icon-button" aria-label={`Editar ${label}`} onClick={() => onEdit(movement)}>
                  <Icon name="pencil" />
                </button>
                <button
                  type="button"
                  className="icon-button icon-button--danger"
                  aria-label={`Excluir ${label}`}
                  onClick={() => onDelete(movement)}
                >
                  <Icon name="trash" />
                </button>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}

function LedgerSkeleton() {
  return (
    <div className="skeleton" aria-busy="true">
      <span className="visually-hidden">Carregando movimentações…</span>
      {[0, 1, 2, 3].map((row) => (
        <div key={row} className="skeleton__row" aria-hidden="true">
          <span className="skeleton__bar skeleton__bar--short" />
          <span className="skeleton__bar" />
          <span className="skeleton__bar skeleton__bar--num" />
        </div>
      ))}
    </div>
  );
}
