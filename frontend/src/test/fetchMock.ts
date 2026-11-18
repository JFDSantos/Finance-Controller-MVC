import { vi } from "vitest";

export type Route = { status: number; body?: unknown };
export type Routes = Record<string, Route | Route[]>;

/** Installs a fetch stub answering by "METHOD /path". Array values are consumed in order (the last one repeats). */
export function mockFetch(routes: Routes) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const key = `${init?.method ?? "GET"} ${String(input)}`;
    const entry = routes[key];
    if (!entry) throw new Error(`Unexpected request: ${key}`);
    const route = Array.isArray(entry) ? (entry.length > 1 ? entry.shift()! : entry[0]) : entry;
    if (route.status === 204) return new Response(null, { status: 204 });
    return new Response(route.body === undefined ? null : JSON.stringify(route.body), {
      status: route.status,
      headers: { "Content-Type": "application/json" },
    });
  });
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

export const demoUser = { id: 7, userName: "Ana Souza", email: "ana@exemplo.com", role: "user" };
