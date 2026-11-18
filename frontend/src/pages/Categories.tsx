import { useEffect, useRef, useState } from "react";
import type { FormEvent, KeyboardEvent } from "react";
import { ApiError } from "../api/client";
import { categoriesApi } from "../api/categories";
import type { Category } from "../api/types";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { Field } from "../components/Field";
import { Icon } from "../components/Icon";
import { Notice } from "../components/Notice";
import { errorMessage } from "../i18n/errors";
import { byName, MAX_CATEGORY_NAME, validateCategoryName } from "../lib/categories";
import { usePageTitle } from "./usePageTitle";

function messageOf(error: unknown): string {
  if (error instanceof ApiError) {
    const field = error.fieldErrors.find((f) => f.propertyName.toLowerCase() === "name");
    if (field?.errorCode === "MaximumLengthValidator") return `Use no máximo ${MAX_CATEGORY_NAME} caracteres.`;
    if (field) return "Informe um nome para a categoria.";
    return errorMessage(error.code);
  }
  return errorMessage("UNKNOWN");
}

type Load = { status: "loading" } | { status: "error"; message: string } | { status: "ready"; data: Category[] };

export default function Categories() {
  usePageTitle("Categorias");
  const [load, setLoad] = useState<Load>({ status: "loading" });
  const [reload, setReload] = useState(0);
  const [announcement, setAnnouncement] = useState<string>();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deleting, setDeleting] = useState<Category | null>(null);
  const listTitleRef = useRef<HTMLHeadingElement>(null);
  const focusAfterRename = useRef<number | null>(null);

  useEffect(() => {
    let active = true;
    setLoad({ status: "loading" });
    categoriesApi
      .list()
      .then((data) => active && setLoad({ status: "ready", data }))
      .catch((error) => active && setLoad({ status: "error", message: messageOf(error) }));
    return () => {
      active = false;
    };
  }, [reload]);

  // After a rename, focus goes back to that row's "Renomear" button.
  useEffect(() => {
    if (editingId !== null || focusAfterRename.current === null) return;
    document.getElementById(`rename-${focusAfterRename.current}`)?.focus();
    focusAfterRename.current = null;
  }, [editingId]);

  const categories = load.status === "ready" ? [...load.data].sort(byName) : [];

  function replace(update: (list: Category[]) => Category[]) {
    setLoad((previous) => (previous.status === "ready" ? { status: "ready", data: update(previous.data) } : previous));
  }

  async function create(name: string) {
    const created = await categoriesApi.create({ name });
    replace((list) => [...list, created]);
    setAnnouncement(`Categoria “${created.name}” criada.`);
  }

  async function rename(category: Category, name: string) {
    await categoriesApi.rename(category.id, { name });
    replace((list) => list.map((c) => (c.id === category.id ? { ...c, name } : c)));
    setAnnouncement(`Categoria renomeada para “${name}”.`);
    focusAfterRename.current = category.id;
    setEditingId(null);
  }

  async function confirmDelete() {
    if (!deleting) return;
    await categoriesApi.remove(deleting.id);
    replace((list) => list.filter((c) => c.id !== deleting.id));
    setAnnouncement(`Categoria “${deleting.name}” excluída.`);
    setDeleting(null);
  }

  return (
    <section className="page categories" aria-labelledby="page-title">
      <header className="page__header">
        <div>
          <h1 className="page__title" id="page-title">
            Categorias
          </h1>
          <p className="page__lead">Cada movimentação pertence a uma categoria. Ajuste a lista ao seu jeito.</p>
        </div>
      </header>

      {load.status === "ready" ? (
        <CreateCategory existing={load.data} onCreate={create} />
      ) : null}

      <div role="status" className="live-region">
        {announcement ? <Notice>{announcement}</Notice> : null}
      </div>

      <section className="categories__panel" aria-labelledby="categories-title">
        <div className="categories__head">
          <h2 className="section-title" id="categories-title" ref={listTitleRef} tabIndex={-1}>
            Suas categorias
          </h2>
          {load.status === "ready" && categories.length > 0 ? (
            <p className="categories__count">
              {categories.length} {categories.length === 1 ? "categoria" : "categorias"}
            </p>
          ) : null}
        </div>

        {load.status === "error" ? (
          <div className="state state--error">
            <Notice tone="danger">{load.message}</Notice>
            <button type="button" className="button button--secondary" onClick={() => setReload((n) => n + 1)}>
              Tentar novamente
            </button>
          </div>
        ) : load.status === "loading" ? (
          <div className="skeleton" aria-busy="true">
            <span className="visually-hidden">Carregando categorias…</span>
            {[0, 1, 2, 3].map((row) => (
              <div key={row} className="skeleton__row" aria-hidden="true">
                <span className="skeleton__bar" />
              </div>
            ))}
          </div>
        ) : categories.length === 0 ? (
          <div className="state">
            <h3 className="state__title">Nenhuma categoria ainda</h3>
            <p className="state__text">
              Crie a primeira no campo acima, por exemplo Moradia ou Salário. Toda movimentação precisa de uma.
            </p>
          </div>
        ) : (
          <ul className="category-list" aria-labelledby="categories-title">
            {categories.map((category) =>
              editingId === category.id ? (
                <RenameRow
                  key={category.id}
                  category={category}
                  existing={categories}
                  onSave={(name) => rename(category, name)}
                  onCancel={() => {
                    focusAfterRename.current = category.id;
                    setEditingId(null);
                  }}
                />
              ) : (
                <li key={category.id} className="category">
                  <span className="category__name">{category.name}</span>
                  <div className="category__actions">
                    <button
                      type="button"
                      id={`rename-${category.id}`}
                      className="button button--quiet"
                      aria-label={`Renomear ${category.name}`}
                      onClick={() => {
                        setAnnouncement(undefined);
                        setEditingId(category.id);
                      }}
                    >
                      <Icon name="pencil" size={18} />
                      <span className="button__text">Renomear</span>
                    </button>
                    <button
                      type="button"
                      className="button button--quiet button--quiet-danger"
                      aria-label={`Excluir ${category.name}`}
                      onClick={() => {
                        setAnnouncement(undefined);
                        setDeleting(category);
                      }}
                    >
                      <Icon name="trash" size={18} />
                      <span className="button__text">Excluir</span>
                    </button>
                  </div>
                </li>
              ),
            )}
          </ul>
        )}
      </section>

      <ConfirmDialog
        open={deleting !== null}
        title={deleting ? `Excluir a categoria “${deleting.name}”?` : ""}
        confirmLabel="Excluir categoria"
        pendingLabel="Excluindo…"
        onConfirm={confirmDelete}
        onCancel={() => setDeleting(null)}
        finalErrors={["CATEGORY_IN_USE"]}
        fallbackFocus={() => listTitleRef.current}
      >
        <p>Só é possível excluir uma categoria que não tem movimentações. Esta ação não pode ser desfeita.</p>
      </ConfirmDialog>
    </section>
  );
}

function CreateCategory({ existing, onCreate }: { existing: Category[]; onCreate(name: string): Promise<void> }) {
  const [name, setName] = useState("");
  const [error, setError] = useState<string>();
  const [pending, setPending] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const problem = validateCategoryName(name, existing);
    setError(problem);
    if (problem) {
      inputRef.current?.focus();
      return;
    }
    setPending(true);
    try {
      await onCreate(name.trim());
      setName("");
    } catch (cause) {
      setError(messageOf(cause));
    } finally {
      setPending(false);
      // Ready for the next one.
      queueMicrotask(() => inputRef.current?.focus());
    }
  }

  return (
    <form className="create-category" noValidate onSubmit={(event) => void submit(event)}>
      <Field
        ref={inputRef}
        label="Nova categoria"
        value={name}
        onChange={(event) => setName(event.target.value)}
        maxLength={MAX_CATEGORY_NAME}
        autoComplete="off"
        placeholder="Ex.: Viagens"
        error={error}
        readOnly={pending}
      />
      <button type="submit" className="button button--primary" disabled={pending} data-pending={pending || undefined}>
        <Icon name="plus" />
        {pending ? "Adicionando…" : "Adicionar"}
      </button>
    </form>
  );
}

function RenameRow({
  category,
  existing,
  onSave,
  onCancel,
}: {
  category: Category;
  existing: Category[];
  onSave(name: string): Promise<void>;
  onCancel(): void;
}) {
  const [name, setName] = useState(category.name);
  const [error, setError] = useState<string>();
  const [pending, setPending] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.focus();
    inputRef.current?.select();
  }, []);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const trimmed = name.trim();
    if (trimmed === category.name) {
      onCancel();
      return;
    }
    const problem = validateCategoryName(name, existing, category.id);
    setError(problem);
    if (problem) {
      inputRef.current?.focus();
      return;
    }
    setPending(true);
    try {
      await onSave(trimmed);
    } catch (cause) {
      setError(messageOf(cause));
      setPending(false);
      inputRef.current?.focus();
    }
  }

  function onKeyDown(event: KeyboardEvent) {
    if (event.key === "Escape" && !pending) {
      event.preventDefault();
      onCancel();
    }
  }

  return (
    <li className="category category--editing">
      <form className="rename" noValidate onSubmit={(event) => void submit(event)} onKeyDown={onKeyDown}>
        <Field
          ref={inputRef}
          label={`Novo nome para ${category.name}`}
          className="rename__field field--label-hidden"
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={MAX_CATEGORY_NAME}
          autoComplete="off"
          error={error}
          readOnly={pending}
        />
        <div className="rename__actions">
          <button type="button" className="button button--secondary" onClick={onCancel} disabled={pending}>
            Cancelar
          </button>
          <button type="submit" className="button button--primary" disabled={pending} data-pending={pending || undefined}>
            {pending ? "Salvando…" : "Salvar"}
          </button>
        </div>
      </form>
    </li>
  );
}
