import type { Category } from "../api/types";

export const MAX_CATEGORY_NAME = 100;

export const byName = (a: Category, b: Category) => a.name.localeCompare(b.name, "pt-BR", { sensitivity: "base" });

/** Mirrors CategoryCreateDtoValidator (not empty, at most 100) plus the per-user unique index, ignoring case. */
export function validateCategoryName(name: string, existing: Category[], selfId?: number): string | undefined {
  const trimmed = name.trim();
  if (!trimmed) return "Informe um nome para a categoria.";
  if (trimmed.length > MAX_CATEGORY_NAME) return `Use no máximo ${MAX_CATEGORY_NAME} caracteres.`;
  const key = trimmed.toLocaleLowerCase("pt-BR");
  if (existing.some((c) => c.id !== selfId && c.name.trim().toLocaleLowerCase("pt-BR") === key)) {
    return "Já existe uma categoria com esse nome.";
  }
  return undefined;
}
