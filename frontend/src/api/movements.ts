import { api } from "./client";
import type { Movement, MovementFilter, MovementInput, MovementType } from "./types";

/** Income and Expense are separate resources on the API. */
const RESOURCE: Record<MovementType, string> = {
  Income: "/api/Income",
  Expense: "/api/Expense",
};

/** `/api/Movement?year=&month=&categoryId=&type=`, parameters always in that order. */
export function movementListPath(filter: MovementFilter): string {
  const params = new URLSearchParams();
  params.set("year", String(filter.year));
  if (filter.month !== undefined) params.set("month", String(filter.month));
  if (filter.categoryId !== undefined) params.set("categoryId", String(filter.categoryId));
  if (filter.type !== undefined) params.set("type", filter.type);
  return `/api/Movement?${params.toString()}`;
}

export const movementsApi = {
  list: (filter: MovementFilter) => api.get<Movement[]>(movementListPath(filter)),
  create: (type: MovementType, input: MovementInput) => api.post<unknown>(RESOURCE[type], input),
  update: (type: MovementType, id: number, input: MovementInput) => api.put<unknown>(`${RESOURCE[type]}/${id}`, input),
  remove: (type: MovementType, id: number) => api.delete(`${RESOURCE[type]}/${id}`),
};
