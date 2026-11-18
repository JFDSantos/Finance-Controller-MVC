import { useId, useRef, useState } from "react";
import type { FormEvent } from "react";
import { ApiError } from "../api/client";
import { movementsApi } from "../api/movements";
import type { Category, Movement, MovementInput, MovementType } from "../api/types";
import { errorMessage } from "../i18n/errors";
import { amountInput, parseAmount } from "../lib/format";
import { Field } from "./Field";
import { Notice } from "./Notice";
import { Segmented } from "./Segmented";
import { SelectField } from "./SelectField";

export const MAX_DESCRIPTION = 100;

export interface SavedMovement {
  type: MovementType;
  movimentDate: string;
  categoryId: number;
}

interface MovementFormProps {
  categories: Category[];
  /** Edit this movement; omit to create one. */
  movement?: Movement;
  /** Date offered for a new movement ("yyyy-MM-dd"). */
  defaultDate: string;
  onSaved(saved: SavedMovement): void;
  onCancel(): void;
}

type FieldName = "description" | "movimentDate" | "categoryId" | "value";
type Errors = Partial<Record<FieldName, string>>;

const TYPE_OPTIONS = [
  { value: "Expense" as const, label: "Despesa" },
  { value: "Income" as const, label: "Receita" },
];

const AMOUNT_FORMAT = "Use um valor como 1.234,50, com até 2 casas decimais.";

function amountError(text: string): string | undefined {
  const trimmed = text.trim();
  if (!trimmed) return "Informe o valor.";
  if (parseAmount(trimmed) !== null) return undefined;
  if (/^-/.test(trimmed) || /^[R$\s0.,]+$/i.test(trimmed)) return `O valor precisa ser maior que zero. ${AMOUNT_FORMAT}`;
  return AMOUNT_FORMAT;
}

function validate(values: { description: string; movimentDate: string; categoryId: string; value: string }): Errors {
  const errors: Errors = {};
  const description = values.description.trim();
  if (!description) errors.description = "Informe uma descrição.";
  else if (description.length > MAX_DESCRIPTION) errors.description = `Use no máximo ${MAX_DESCRIPTION} caracteres.`;
  if (!/^\d{4}-\d{2}-\d{2}$/.test(values.movimentDate)) errors.movimentDate = "Informe a data.";
  if (!values.categoryId) errors.categoryId = "Escolha uma categoria.";
  const value = amountError(values.value);
  if (value) errors.value = value;
  return errors;
}

/** Server field errors (FluentValidation property names) mapped onto this form's fields. */
function serverFieldErrors(error: ApiError): Errors {
  const errors: Errors = {};
  for (const field of error.fieldErrors) {
    const property = field.propertyName.toLowerCase();
    if (field.errorCode === "CATEGORY_NOT_FOUND" || property === "categoryid") {
      errors.categoryId = errorMessage("CATEGORY_NOT_FOUND");
    } else if (property === "description") {
      errors.description = field.errorCode === "MaximumLengthValidator"
        ? `Use no máximo ${MAX_DESCRIPTION} caracteres.`
        : "Informe uma descrição.";
    } else if (property === "value") {
      errors.value = `O valor precisa ser maior que zero. ${AMOUNT_FORMAT}`;
    } else if (property === "movimentdate") {
      errors.movimentDate = "Informe uma data válida.";
    }
  }
  return errors;
}

const ORDER: FieldName[] = ["description", "movimentDate", "categoryId", "value"];

export function MovementForm({ categories, movement, defaultDate, onSaved, onCancel }: MovementFormProps) {
  const editing = movement !== undefined;
  const [type, setType] = useState<MovementType>(movement?.type ?? "Expense");
  const [description, setDescription] = useState(movement?.description ?? "");
  const [movimentDate, setMovimentDate] = useState(movement?.movimentDate ?? defaultDate);
  const [categoryId, setCategoryId] = useState(movement ? String(movement.categoryId) : "");
  const [value, setValue] = useState(movement ? amountInput(movement.value) : "");
  const [isRecurring, setIsRecurring] = useState(movement?.isRecurring ?? false);
  const [errors, setErrors] = useState<Errors>({});
  const [formError, setFormError] = useState<string>();
  const [pending, setPending] = useState(false);
  const recurringId = useId();
  const refs = {
    description: useRef<HTMLInputElement>(null),
    movimentDate: useRef<HTMLInputElement>(null),
    categoryId: useRef<HTMLSelectElement>(null),
    value: useRef<HTMLInputElement>(null),
  };

  // The movement's own category stays selectable even if it is no longer in the list.
  const options =
    movement && !categories.some((c) => c.id === movement.categoryId)
      ? [...categories, { id: movement.categoryId, name: movement.categoryName }]
      : categories;

  function focusFirst(found: Errors) {
    const first = ORDER.find((name) => found[name]);
    if (first) refs[first].current?.focus();
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const found = validate({ description, movimentDate, categoryId, value });
    setErrors(found);
    setFormError(undefined);
    if (Object.keys(found).length > 0) {
      focusFirst(found);
      return;
    }

    const input: MovementInput = {
      description: description.trim(),
      movimentDate,
      isRecurring,
      categoryId: Number(categoryId),
      value: parseAmount(value)!,
    };
    setPending(true);
    try {
      if (movement) await movementsApi.update(movement.type, movement.id, input);
      else await movementsApi.create(type, input);
      onSaved({ type, movimentDate: input.movimentDate, categoryId: input.categoryId });
    } catch (cause) {
      const error = cause instanceof ApiError ? cause : new ApiError(0, "UNKNOWN");
      const fieldErrors = serverFieldErrors(error);
      if (Object.keys(fieldErrors).length > 0) {
        setErrors(fieldErrors);
        focusFirst(fieldErrors);
      } else {
        setFormError(errorMessage(error.code));
      }
      setPending(false);
    }
  }

  return (
    <form className="movement-form" noValidate onSubmit={(event) => void submit(event)}>
      <Segmented<MovementType>
        legend="Tipo"
        options={TYPE_OPTIONS}
        value={type}
        onChange={setType}
        disabled={editing || pending}
        hint={editing ? "O tipo não muda depois de salvo. Para trocar, exclua e registre de novo." : undefined}
        className="segmented--type"
      />

      <Field
        ref={refs.description}
        label="Descrição"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        maxLength={MAX_DESCRIPTION}
        autoComplete="off"
        placeholder={type === "Income" ? "Ex.: Salário" : "Ex.: Mercado da semana"}
        error={errors.description}
        disabled={pending}
      />

      <div className="movement-form__pair">
        <Field
          ref={refs.value}
          label="Valor"
          inputMode="decimal"
          autoComplete="off"
          placeholder="0,00"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          hint="Em reais, como 1.234,50."
          error={errors.value}
          disabled={pending}
          className="field--amount"
        />
        <Field
          ref={refs.movimentDate}
          label="Data"
          type="date"
          value={movimentDate}
          onChange={(e) => setMovimentDate(e.target.value)}
          error={errors.movimentDate}
          disabled={pending}
        />
      </div>

      <SelectField
        ref={refs.categoryId}
        label="Categoria"
        value={categoryId}
        onChange={(e) => setCategoryId(e.target.value)}
        error={errors.categoryId}
        disabled={pending}
      >
        <option value="" disabled>
          Escolha uma categoria
        </option>
        {options.map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </SelectField>

      <div className="checkbox">
        <input
          id={recurringId}
          type="checkbox"
          checked={isRecurring}
          onChange={(e) => setIsRecurring(e.target.checked)}
          disabled={pending}
          aria-describedby={`${recurringId}-note`}
        />
        <div>
          <label htmlFor={recurringId}>Recorrente</label>
          <p className="checkbox__note" id={`${recurringId}-note`}>
            Marca {type === "Income" ? "uma receita" : "uma despesa"} que se repete todo mês, como aluguel ou salário.
            Não cria lançamentos automáticos.
          </p>
        </div>
      </div>

      <div className="live-region" aria-live="assertive">
        {formError ? <Notice tone="danger">{formError}</Notice> : null}
      </div>

      <div className="dialog__actions">
        <button type="button" className="button button--secondary" onClick={onCancel} disabled={pending}>
          Cancelar
        </button>
        <button type="submit" className="button button--primary" disabled={pending} data-pending={pending || undefined}>
          {pending ? "Salvando…" : editing ? "Salvar alterações" : "Salvar"}
        </button>
      </div>
    </form>
  );
}
