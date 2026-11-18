import { api } from "./client";
import type { Category, CategoryInput } from "./types";

const BASE = "/api/Category";

export const categoriesApi = {
  list: () => api.get<Category[]>(BASE),
  create: (input: CategoryInput) => api.post<Category>(BASE, input),
  rename: (id: number, input: CategoryInput) => api.put<Category>(`${BASE}/${id}`, input),
  remove: (id: number) => api.delete(`${BASE}/${id}`),
};
